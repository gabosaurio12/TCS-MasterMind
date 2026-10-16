using log4net;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Services;
using System.Data.Entity.Core;
using System.Linq;

namespace MasterMind_Server.Repositories
{
    internal class PlayerRepository
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(PlayerRepository));
        private readonly static MasterMindEntities Context = new MasterMindEntities(true);

        public static (bool, string) CreatePlayer(Player player)
        {
            try
            {
                if (Context.Player.Any(p => p.username == player.username))
                {
                    return (false, RepositoriesEnum.UsernameTaken.ToString());
                }
                if (Context.Player.Any(p => p.email == player.email))
                {
                    return (false, RepositoriesEnum.EmailTaken.ToString());
                }

                player.password = BCrypt.Net.BCrypt.HashPassword(player.password);

                Context.Player.Add(player);
                Context.SaveChanges();

                return (true, RepositoriesEnum.Success.ToString());
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return (false, RepositoriesEnum.Error.ToString());
        }

        public static Player GetPlayerByUsername(string username)
        {
            try
            {
                var player = Context.Player.FirstOrDefault(p => p.username == username);
                if (player != null)
                {
                    return player;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return null;
        }

        public static Player GetPlayerById(int playerId)
        {
            try
            {
                var player = Context.Player.FirstOrDefault(p => p.player_id == playerId);
                if (player != null)
                {
                    return player;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return null;
        }
    }
}
