using MasterMind_Server.Contracts;
using MasterMind_Server.Repositories.ClassHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal sealed class RoundState
    {
        private const int PlayersPerRound = 2;
        private const double ExpiryToleranceMs = 100;

        private readonly object sync = new object();
        private readonly Dictionary<string, int> triesByUser =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, OpponentProgress> finishedByUser =
            new Dictionary<string, OpponentProgress>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> readyUsers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool IsResolved { get; private set; }

        public DateTime? StartedAtUtc { get; private set; }

        public DateTime? DeadlineUtc { get; private set; }

        public bool IsClockRunning
        {
            get
            {
                lock (sync)
                {
                    return DeadlineUtc.HasValue;
                }
            }
        }

        public static RoundState Restore(MatchRoundRecord record, MatchRoomInfo info)
        {
            var round = new RoundState
            {
                StartedAtUtc = record.StartedAtUtc,
                DeadlineUtc = record.DeadlineUtc
            };
            round.triesByUser[info.PlayerOneUsername] = info.PlayerOneTries;
            round.triesByUser[info.PlayerTwoUsername] = info.PlayerTwoTries;
            if (record.FinishedUsername != null)
            {
                round.finishedByUser[record.FinishedUsername] = new OpponentProgress
                {
                    Username = record.FinishedUsername,
                    Solved = record.FinishedSolved,
                    TriesUsed = record.FinishedTries
                };
            }

            return round;
        }

        public OpponentProgress GetFirstFinished()
        {
            lock (sync)
            {
                return finishedByUser.Values.FirstOrDefault();
            }
        }

        public bool TryStartClock(string username, TimeSpan duration)
        {
            lock (sync)
            {
                readyUsers.Add(username);
                if (readyUsers.Count < PlayersPerRound || DeadlineUtc.HasValue || IsResolved)
                {
                    return false;
                }

                StartedAtUtc = DateTime.UtcNow;
                DeadlineUtc = StartedAtUtc + duration;
                return true;
            }
        }

        public int GetElapsedSeconds()
        {
            lock (sync)
            {
                if (!StartedAtUtc.HasValue)
                {
                    return 1;
                }

                var elapsed = (DateTime.UtcNow - StartedAtUtc.Value).TotalSeconds;
                return Math.Max(1, (int)Math.Ceiling(elapsed));
            }
        }

        public int GetRemainingMs()
        {
            lock (sync)
            {
                if (!DeadlineUtc.HasValue)
                {
                    return 0;
                }

                return (int)Math.Max(0, (DeadlineUtc.Value - DateTime.UtcNow).TotalMilliseconds);
            }
        }

        public RoundResult SolveFirst(OpponentProgress winner, string opponentUsername)
        {
            lock (sync)
            {
                if (IsResolved || !DeadlineUtc.HasValue)
                {
                    return CreatePendingResult();
                }

                IsResolved = true;
                return BuildTimeTrialResult(winner.Username, opponentUsername, winner.Username);
            }
        }

        public RoundResult Expire(string firstUsername, string secondUsername)
        {
            lock (sync)
            {
                bool isOver = DeadlineUtc.HasValue
                    && (DeadlineUtc.Value - DateTime.UtcNow).TotalMilliseconds <= ExpiryToleranceMs;
                if (IsResolved || !isOver)
                {
                    return CreatePendingResult();
                }

                IsResolved = true;
                return BuildTimeTrialResult(firstUsername, secondUsername, null);
            }
        }

        public static RoundResult CreatePendingResult()
        {
            return new RoundResult { Players = new OpponentProgress[0] };
        }

        public static bool IsPending(RoundResult result)
        {
            return result.Players.Length == 0;
        }

        public int CountTry(string username)
        {
            lock (sync)
            {
                if (finishedByUser.ContainsKey(username))
                {
                    return 0;
                }

                triesByUser.TryGetValue(username, out var tries);
                triesByUser[username] = tries + 1;
                return tries + 1;
            }
        }

        public RoundResult Finish(OpponentProgress progress)
        {
            lock (sync)
            {
                finishedByUser[progress.Username] = progress;
                if (finishedByUser.Count < PlayersPerRound || IsResolved)
                {
                    return CreatePendingResult();
                }

                IsResolved = true;
                return BuildResult();
            }
        }

        private RoundResult BuildTimeTrialResult(string firstUsername, string secondUsername, string winner)
        {
            var players = new[] { firstUsername, secondUsername }.Select(username =>
            {
                triesByUser.TryGetValue(username, out var tries);
                return new OpponentProgress { Username = username, Solved = username == winner, TriesUsed = tries };
            });

            return new RoundResult { WinnerUsername = winner, Players = players.ToArray() };
        }

        private RoundResult BuildResult()
        {
            var players = finishedByUser.Values.ToArray();
            return new RoundResult
            {
                WinnerUsername = DetermineWinner(players[0], players[1]),
                Players = players
            };
        }

        private static string DetermineWinner(OpponentProgress first, OpponentProgress second)
        {
            if (first.Solved && second.Solved)
            {
                return DetermineFasterWinner(first, second);
            }

            if (first.Solved || second.Solved)
            {
                return first.Solved ? first.Username : second.Username;
            }

            return null;
        }

        private static string DetermineFasterWinner(OpponentProgress first, OpponentProgress second)
        {
            if (first.TriesUsed == second.TriesUsed)
            {
                return null;
            }

            return first.TriesUsed < second.TriesUsed ? first.Username : second.Username;
        }
    }
}
