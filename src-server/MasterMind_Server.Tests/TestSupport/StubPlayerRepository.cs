using MasterMind_Server.Data;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;

namespace MasterMind_Server.Tests.TestSupport
{
    internal class StubPlayerRepository : IPlayerRepository
    {
        private readonly PlayerRepository innerRepository = new PlayerRepository();

        public string UpdateAvatarPathResult { get; set; }

        public string CreatePlayer(Player player)
        {
            return innerRepository.CreatePlayer(player);
        }

        public Player GetPlayerByUsername(string username)
        {
            return innerRepository.GetPlayerByUsername(username);
        }

        public Player GetPlayerById(int playerId)
        {
            return innerRepository.GetPlayerById(playerId);
        }

        public PlayerRecordsDto GetPlayerRecords(string username)
        {
            return innerRepository.GetPlayerRecords(username);
        }

        public string UpdatePlayer(Player player)
        {
            return innerRepository.UpdatePlayer(player);
        }

        public string UpdatePassword(string username, string password)
        {
            return innerRepository.UpdatePassword(username, password);
        }

        public string UpdateAvatarPath(int playerId, string fileName)
        {
            return UpdateAvatarPathResult ?? innerRepository.UpdateAvatarPath(playerId, fileName);
        }

        public string ReportPlayer(ReportDto report)
        {
            return innerRepository.ReportPlayer(report);
        }
    }
}
