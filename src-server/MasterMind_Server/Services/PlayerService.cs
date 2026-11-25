using BCrypt.Net;
using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Services.ErrorHelpers;
using MasterMind_Server.Services.GuestHelpers;
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
                    return SendRegistrationCode(playerEntity.username);
                }

                return result;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw new FaultException(DbFaultClassifier.Classify(ex).ToString());
            }
        }

        private string SendRegistrationCode(string username)
        {
            var playerRegistered = GetPlayerByUsername(username);

            try
            {
                new CodeService().SendVerificationCode(playerRegistered.PlayerId);
            }
            catch (FaultException<ErrorMessage> ex) when (IsEmailFault(ex.Detail.Message))
            {
                logger.Warn("The account was created but the verification code could not be emailed.", ex);
                return RepositoriesMessages.SuccessEmailFailed.ToString();
            }

            return RepositoriesMessages.Success.ToString();
        }

        private static bool IsEmailFault(string message)
        {
            return message == ErrorMessages.SendEmailError.ToString()
                || message == ErrorMessages.SmtpServerError.ToString();
        }

        public bool LoginPlayer(string username, string password)
        {
            var playerService = new PlayerService();
            var player = playerService.GetPlayerByUsername(username);
            if (player.PlayerId != 0 && !playerRepository.IsGuest(username))
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
                        AvatarPath = playerEntity.avatar_path,
                        IsOnline = playerEntity.is_online == PlayerRepository.OnlineValue
                    };
                    return player;
                }

                return player;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw new FaultException(DbFaultClassifier.Classify(ex).ToString());
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
                        AvatarPath = playerEntity.avatar_path,
                        IsOnline = playerEntity.is_online == PlayerRepository.OnlineValue
                    };
                    return player;
                }

                return player;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw new FaultException(DbFaultClassifier.Classify(ex).ToString());

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
                throw new FaultException(DbFaultClassifier.Classify(ex).ToString());

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
                throw new FaultException(DbFaultClassifier.Classify(ex).ToString());

            }
        }

        public string ChangePassword(string username, string password)
        {
            try
            {
                var result = playerRepository.UpdatePassword(username, password);

                return result;
            }
            catch (EntityException ex)
            {
                throw new FaultException(DbFaultClassifier.Classify(ex).ToString());
            }
        }

        public void ReportPlayer(Report report)
        {
            if (report == null)
            {
                throw BuildFault(ErrorMessages.InvalidReportReason);
            }

            if (playerRepository.IsGuest(report.ReportedByPlayerUsername))
            {
                throw BuildFault(ErrorMessages.GuestCannotReport);
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
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        public string SetOnlineStatus(int playerId, bool isOnline)
        {
            try
            {
                var result = playerRepository.UpdateOnlineStatus(playerId, isOnline);

                if (result == RepositoriesMessages.PlayerNotFound.ToString())
                {
                    throw BuildFault(ErrorMessages.PlayerNotFound);
                }

                return result;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        public string CreateGuest(string username)
        {
            try
            {
                new GuestRemover().PurgeOffline();

                return playerRepository.CreateGuest(username?.Trim());
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        public void DeleteGuest(int playerId)
        {
            try
            {
                var result = new GuestRemover().Remove(playerId);
                if (result == RepositoriesMessages.PlayerNotFound.ToString())
                {
                    throw BuildFault(ErrorMessages.PlayerNotFound);
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
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
