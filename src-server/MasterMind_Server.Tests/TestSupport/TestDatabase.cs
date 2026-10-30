using log4net;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace MasterMind_Server.Tests.TestSupport
{
    internal static class TestDatabase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TestDatabase));

        public static void CleanupTestData()
        {
            using (var context = new MasterMindEntities(true))
            {
                string connectionString = context.Database.Connection.ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            ExecuteCleanup(connection, transaction);
                            transaction.Commit();
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            logger.Error("Failed to execute test database cleanup.", ex);
                            throw;
                        }
                        catch (InvalidOperationException ex)
                        {
                            transaction.Rollback();
                            logger.Error("Failed to execute test database cleanup.", ex);
                            throw;
                        }
                    }
                }
            }
        }

        private static void ExecuteCleanup(SqlConnection connection, SqlTransaction transaction)
        {
            string sql = @"
                DECLARE @Prefix NVARCHAR(50) = 'servertest_%';

                CREATE TABLE #TestPlayers (player_id INT PRIMARY KEY);
                CREATE TABLE #TestRooms (match_room_id INT PRIMARY KEY);

                INSERT INTO #TestPlayers
                SELECT player_id FROM dbo.Player WHERE username LIKE @Prefix;

                INSERT INTO #TestRooms
                SELECT match_room_id FROM dbo.MatchRoom
                WHERE match_room_name LIKE @Prefix
                   OR player_one_id IN (SELECT player_id FROM #TestPlayers)
                   OR player_two_id IN (SELECT player_id FROM #TestPlayers);

                DELETE FROM dbo.CorrectPositionAndColorsClue
                WHERE correct_position_and_colors_id IN (
                    SELECT decipher_try_id FROM dbo.DecipherTry
                    WHERE match_room_id IN (SELECT match_room_id FROM #TestRooms)
                       OR player_id IN (SELECT player_id FROM #TestPlayers));

                DELETE FROM dbo.DecipherTryCode
                WHERE decipher_try_id IN (
                    SELECT decipher_try_id FROM dbo.DecipherTry
                    WHERE match_room_id IN (SELECT match_room_id FROM #TestRooms)
                       OR player_id IN (SELECT player_id FROM #TestPlayers));

                DELETE FROM dbo.CorrectPositionAndColors
                WHERE decipher_try_id IN (
                    SELECT decipher_try_id FROM dbo.DecipherTry
                    WHERE match_room_id IN (SELECT match_room_id FROM #TestRooms)
                       OR player_id IN (SELECT player_id FROM #TestPlayers));

                DELETE FROM dbo.DecipherTry
                WHERE match_room_id IN (SELECT match_room_id FROM #TestRooms)
                   OR player_id IN (SELECT player_id FROM #TestPlayers);

                DELETE FROM dbo.GameInvitation
                WHERE requester_id IN (SELECT player_id FROM #TestPlayers)
                   OR addressee_id IN (SELECT player_id FROM #TestPlayers)
                   OR match_room_id IN (SELECT match_room_id FROM #TestRooms);

                DELETE FROM dbo.MatchResults
                WHERE match_room_id IN (SELECT match_room_id FROM #TestRooms)
                   OR player_one_results_id IN (
                       SELECT player_in_match_id FROM dbo.PlayerInMatch
                       WHERE player_id IN (SELECT player_id FROM #TestPlayers))
                   OR player_two_results_id IN (
                       SELECT player_in_match_id FROM dbo.PlayerInMatch
                       WHERE player_id IN (SELECT player_id FROM #TestPlayers));

                DELETE FROM dbo.RoundsTimeResults
                WHERE player_in_match_id IN (
                    SELECT player_in_match_id FROM dbo.PlayerInMatch
                    WHERE player_id IN (SELECT player_id FROM #TestPlayers));

                DELETE FROM dbo.RoundsTriesResults
                WHERE player_in_match_id IN (
                    SELECT player_in_match_id FROM dbo.PlayerInMatch
                    WHERE player_id IN (SELECT player_id FROM #TestPlayers));

                DELETE FROM dbo.PlayerInMatch
                WHERE player_id IN (SELECT player_id FROM #TestPlayers);

                DELETE FROM dbo.TimeTrialConfig
                WHERE match_room_id IN (SELECT match_room_id FROM #TestRooms);

                DELETE FROM dbo.TriesConfig
                WHERE match_room_id IN (SELECT match_room_id FROM #TestRooms);

                DELETE FROM dbo.MatchRoomSecretCode
                WHERE match_room_id IN (SELECT match_room_id FROM #TestRooms);

                DELETE FROM dbo.PlayerReport
                WHERE reported_player_id IN (SELECT player_id FROM #TestPlayers)
                   OR reported_by_player_id IN (SELECT player_id FROM #TestPlayers);

                DELETE FROM dbo.VerificationCode
                WHERE player_id IN (SELECT player_id FROM #TestPlayers);

                DELETE FROM dbo.Friendship
                WHERE requester_id IN (SELECT player_id FROM #TestPlayers)
                   OR addressee_id IN (SELECT player_id FROM #TestPlayers);

                DELETE FROM dbo.PlayerTriesRecords
                WHERE player_id IN (SELECT player_id FROM #TestPlayers);

                DELETE FROM dbo.PlayerTimeTrialRecords
                WHERE player_id IN (SELECT player_id FROM #TestPlayers);

                DELETE FROM dbo.MatchRoom
                WHERE match_room_id IN (SELECT match_room_id FROM #TestRooms);

                DELETE FROM dbo.Player
                WHERE player_id IN (SELECT player_id FROM #TestPlayers);

                IF EXISTS (SELECT 1 FROM dbo.Player WHERE username LIKE @Prefix)
                    THROW 51000, 'Test players were not fully removed.', 1;

                IF EXISTS (
                    SELECT 1 FROM dbo.MatchRoom
                    WHERE match_room_id IN (SELECT match_room_id FROM #TestRooms))
                    THROW 51001, 'Test rooms were not fully removed.', 1;
            ";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.ExecuteNonQuery();
            }
        }

        public static string GetVerificationCode(int playerId)
        {
            using (var context = new MasterMindEntities(true))
            {
                var code = context.VerificationCode.FirstOrDefault(vc => vc.player_id == playerId);
                return code?.verification_code;
            }
        }

        public static int? GetRoomPlayerOneId(int roomId)
        {
            using (var context = new MasterMindEntities(true))
            {
                return context.MatchRoom
                    .Where(room => room.match_room_id == roomId)
                    .Select(room => room.player_one_id)
                    .FirstOrDefault();
            }
        }

        public static int? GetRoomPlayerTwoId(int roomId)
        {
            using (var context = new MasterMindEntities(true))
            {
                return context.MatchRoom
                    .Where(room => room.match_room_id == roomId)
                    .Select(room => room.player_two_id)
                    .FirstOrDefault();
            }
        }

        public static bool DoesPlayerExist(string username)
        {
            using (var context = new MasterMindEntities(true))
            {
                return context.Player.Any(p => p.username == username);
            }
        }

        public static int CountVerificationCodes(int playerId)
        {
            using (var context = new MasterMindEntities(true))
            {
                return context.VerificationCode.Count(vc => vc.player_id == playerId);
            }
        }

        public static bool DoesVerificationCodeExist(int playerId)
        {
            using (var context = new MasterMindEntities(true))
            {
                return context.VerificationCode.Any(vc => vc.player_id == playerId);
            }
        }

        public static bool DoesReportExist(string reportedUsername)
        {
            using (var context = new MasterMindEntities(true))
            {
                var player = context.Player.FirstOrDefault(p => p.username == reportedUsername);
                return player != null
                    && context.PlayerReport.Any(report => report.reported_player_id == player.player_id);
            }
        }

        public static Data.Player CreatePlayer(string username, string email, string password)
        {
            var repository = new PlayerRepository();
            var player = new Data.Player
            {
                username = username,
                email = email,
                password = password
            };
            repository.CreatePlayer(player);
            return GetPlayerByUsername(username);
        }

        public static Data.Player GetPlayerByUsername(string username)
        {
            using (var context = new MasterMindEntities(true))
            {
                return context.Player.FirstOrDefault(p => p.username == username);
            }
        }
    }
}
