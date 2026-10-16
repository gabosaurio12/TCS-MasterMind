using BCrypt.Net;
using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class PlayerService : IPlayerService
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(PlayerService));

        public (bool, string) RegisterPlayer(Player player)
        {
            var playerEntity = new Data.Player
            {
                username = player.Username,
                email = player.Email,
                password = player.Password
            };

            var result = PlayerRepository.CreatePlayer(playerEntity);

            if (result.Item1)
            {
                var playerRegistered = GetPlayerByUsername(playerEntity.username);
                var authService = new CodeService();
                authService.SendVerificationCode(playerRegistered.PlayerId);
            }

            return result;
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
                    var clientMessage = new ErrorMessage
                    {
                        Message = ErrorMessagesEnum.HashError.ToString()
                    };
                    throw new FaultException<ErrorMessage>(clientMessage);
                }
            }

            return false;
        }

        public Player GetPlayerByUsername(string username)
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
    }
}
