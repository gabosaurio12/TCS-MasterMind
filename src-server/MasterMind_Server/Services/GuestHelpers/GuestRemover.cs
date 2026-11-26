using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.ClassHelpers;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Services.MatchHelpers;
using System;

namespace MasterMind_Server.Services.GuestHelpers
{
    internal class GuestRemover
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(GuestRemover));

        private readonly PlayerRepository playerRepository = new PlayerRepository();
        private readonly MatchRepository matchRepository = new MatchRepository();
        private readonly RoomRepository roomRepository = new RoomRepository();

        public string Remove(int playerId)
        {
            var username = playerRepository.GetGuestUsername(playerId);
            if (username == null)
            {
                return RepositoriesMessages.PlayerNotFound.ToString();
            }

            LeaveRoom(username);

            return playerRepository.DeleteGuest(playerId);
        }

        public void PurgeOffline()
        {
            foreach (var guestId in playerRepository.GetOfflineGuestIds())
            {
                Remove(guestId);
            }
        }

        private void LeaveRoom(string username)
        {
            MatchParticipation participation;
            try
            {
                participation = matchRepository.GetParticipation(username);
            }
            catch (InvalidOperationException ex)
            {
                logger.Debug($"The guest {username} is not in a room.", ex);
                return;
            }

            AbandonMatch(participation, username);
            EscapeFromRoom(participation.RoomId, username);
            MatchCallbackRegistry.Remove(username);
        }

        private static void AbandonMatch(MatchParticipation participation, string username)
        {
            if (MatchCallbackRegistry.TryAbandonRound(participation.RoomId))
            {
                new MatchRoundPersistence().Delete(participation.RoomId);
                MatchCallbackRegistry.Notify(
                    participation.OpponentUsername, callback => callback.OnOpponentAbandonedCallback(username));
            }
        }

        private void EscapeFromRoom(int roomId, string username)
        {
            var status = new PlayerStatus
            {
                RoomId = roomId,
                Username = username,
                Status = PlayerPossibleStatus.Escaped.ToString()
            };

            var result = roomRepository.ChangePlayerStatus(status);
            if (result.Status == RoomResult.Success.ToString())
            {
                LobbyService.NotifyStatusChanges(status, result);
            }
        }
    }
}
