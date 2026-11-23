using BCrypt.Net;
using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Repositories.Enums;
using System.Data.Entity.Core;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class PlayerService : IPlayerService
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(PlayerService));
        private readonly PlayerRepository playerRepository = new PlayerRepository();

        public string RegisterPlayer(Player player)
        {
            var playerEntity = new Data.Player
            {
                username = player.Username,
                email = player.Email,
                password = player.Password
            };

            try
            {
                var result = playerRepository.CreatePlayer(playerEntity);

                if (result.Equals(RepositoriesMessages.Success.ToString()))
                {
                    var playerRegistered = GetPlayerByUsername(playerEntity.username);
                    var authService = new CodeService();
                    authService.SendVerificationCode(playerRegistered.PlayerId);
                }

                return result;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw new FaultException(ErrorMessages.DataBaseError.ToString());
            }
        }

        public bool LoginPlayer(string username, string password)
        {
            var playerService = new PlayerService();
            var player = playerService.GetPlayerByUsername(username);
            if (player.PlayerId != 0)
            {
                try
                {
                    if (BCrypt.Net.BCrypt.Verify(password, player.Password))
                    {
                        var authService = new CodeService();
                        authService.SendVerificationCode(player.PlayerId);
                        return true;
                    }
                }
                catch (SaltParseException ex)
                {
                    logger.Error(ex);
                    throw new FaultException(ErrorMessages.HashError.ToString());
                }
            }

            return false;
        }

        public Player GetPlayerByUsername(string username)
        {
            try
            {
                var playerEntity = playerRepository.GetPlayerByUsername(username);

                var player = new Player
                {
                    PlayerId = 0
                };

                if (playerEntity != null)
                {
                    player = new Player
                    {
                        PlayerId = playerEntity.player_id,
                        Username = playerEntity.username,
                        Email = playerEntity.email,
                        Password = playerEntity.password,
                        AvatarPath = playerEntity.avatar_path
                    };
                    return player;
                }

                return player;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw new FaultException(ErrorMessages.DataBaseError.ToString());
            }
            
        }

        public Player GetPlayerById(int playerId)
        {
            try
            {
                var playerEntity = playerRepository.GetPlayerById(playerId);

                var player = new Player
                {
                    PlayerId = 0
                };

                if (playerEntity != null)
                {
                    player = new Player
                    {
                        PlayerId = playerEntity.player_id,
                        Username = playerEntity.username,
                        Email = playerEntity.email,
                        Password = playerEntity.password,
                        AvatarPath = playerEntity.avatar_path
                    };
                    return player;
                }

                return player;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw new FaultException(ErrorMessages.DataBaseError.ToString());

            }
        }

        public string UpdatePlayer(Player player)
        {
            var playerEntity = new Data.Player
            {
                player_id = player.PlayerId,
                username = player.Username,
                email = player.Email
            };

            try
            {
                var result = playerRepository.UpdatePlayer(playerEntity);
                
                return result;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw new FaultException(ErrorMessages.DataBaseError.ToString());

            }
        }

        public PlayerRecords GetPlayerRecords(string username)
        {
            try
            {
                var playerRecordsDto = playerRepository.GetPlayerRecords(username);

                var playerRecords = new PlayerRecords
                {
                    PlayerUsername = ""
                };

                if (playerRecordsDto != null)
                {
                    playerRecords.PlayerUsername = playerRecordsDto.PlayerUsername;
                    playerRecords.TriesRecords = playerRecordsDto.PlayerTriesRecords;
                    playerRecords.TimeTrialRecords = playerRecordsDto.PlayerTimeTrialRecords;
                }

                return playerRecords;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw new FaultException(ErrorMessages.DataBaseError.ToString());

            }
        }

        public string ChangePassword(string username, string password)
        {
            try
            {
                var result = playerRepository.UpdatePassword(username, password);

                return result;
            }
            catch (EntityException)
            {
                throw new FaultException(ErrorMessages.DataBaseError.ToString());
            }
        }

        public void ReportPlayer(Report report)
        {
            if (report == null)
            {
                throw BuildFault(ErrorMessages.InvalidReportReason);
            }

            var reportDto = new ReportDto
            {
                ReportedPlayerUsername = report.ReportedPlayerUsername,
                ReportedByPlayerUsername = report.ReportedByPlayerUsername,
                Reason = report.Reason
            };

            try
            {
                var result = playerRepository.ReportPlayer(reportDto);
                ThrowIfReportRejected(result);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(ErrorMessages.DataBaseError);
            }
        }

        private static void ThrowIfReportRejected(string result)
        {
            if (result == RepositoriesMessages.Success.ToString())
            {
                return;
            }

            var reason = ErrorMessages.DataBaseError;

            if (result == RepositoriesMessages.PlayerNotFound.ToString())
            {
                reason = ErrorMessages.PlayerNotFound;
            }
            else if (result == RepositoriesMessages.SelfReport.ToString())
            {
                reason = ErrorMessages.SelfReport;
            }
            else if (result == RepositoriesMessages.InvalidReportReason.ToString())
            {
                reason = ErrorMessages.InvalidReportReason;
            }
            else if (result == RepositoriesMessages.AlreadyReported.ToString())
            {
                reason = ErrorMessages.AlreadyReported;
            }

            throw BuildFault(reason);
        }

        private static FaultException<ErrorMessage> BuildFault(ErrorMessages reason)
        {
            var clientMessage = new ErrorMessage { Message = reason.ToString() };

            return new FaultException<ErrorMessage>(clientMessage);
        }
    }
}
