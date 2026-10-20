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
        private ErrorMessage clientMessage = new ErrorMessage();

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
                var result = PlayerRepository.CreatePlayer(playerEntity);

                if (result.Equals(RepositoriesEnum.Success.ToString()))
                {
                    var playerRegistered = GetPlayerByUsername(playerEntity.username);
                    var authService = new CodeService();
                    authService.SendVerificationCode(playerRegistered.PlayerId);
                }

                return result;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public bool LoginPlayer(string username, string password)
        {
            var playerService = new PlayerService();
            var player = playerService.GetPlayerByUsername(username);
            if (player != null)
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
                    clientMessage.Message = ErrorMessagesEnum.HashError.ToString();
                    throw new FaultException<ErrorMessage>(clientMessage);
                }
            }

            return false;
        }

        public Player GetPlayerByUsername(string username)
        {
            try
            {
                var playerEntity = PlayerRepository.GetPlayerByUsername(username);

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
                        Password = playerEntity.password
                    };
                    return player;
                }

                return player;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
            
        }

        public Player GetPlayerById(int playerId)
        {
            try
            {
                var playerEntity = PlayerRepository.GetPlayerById(playerId);

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
                        Password = playerEntity.password
                    };
                    return player;
                }

                return player;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
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
                var result = PlayerRepository.UpdatePlayer(playerEntity);
                
                return result;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public PlayerRecords GetPlayerRecords(string username)
        {
            try
            {
                var playerRecordsDto = PlayerRepository.GetPlayerRecords(username);

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
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public string ChangePassword(string username, string password)
        {
            try
            {
                var result = PlayerRepository.UpdatePassword(username, password);

                return result;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public void ReportPlayer(Report report)
        {
            var reportDto = new ReportDto
            {
                ReportedPlayerUsername = report.ReportedPlayerUsername,
                ReportedByPlayerUsername = report.ReportedByPlayerUsername,
                Reason = report.Reason
            };

            try
            {
                PlayerRepository.ReportPlayer(reportDto);
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }
    }
}
