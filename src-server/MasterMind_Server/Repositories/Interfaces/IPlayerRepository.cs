using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMind_Server.Repositories.Interfaces
{
    public interface IPlayerRepository
    {
        string CreatePlayer(Player player);

        Player GetPlayerByUsername(string username);

        Player GetPlayerById(int playerId);

        PlayerRecordsDto GetPlayerRecords(string username);

        string UpdatePlayer(Player player);

        string UpdatePassword(string username, string password);

        string UpdateAvatarPath(int playerId, string fileName);

        string ReportPlayer(ReportDto report);

        string UpdateOnlineStatus(int playerId, bool isOnline);

        void ResetAllOnlineStatus();

        string CreateGuest(string username);

        string DeleteGuest(int playerId);

        bool IsGuest(string username);

        string GetGuestUsername(int playerId);

        List<int> GetOfflineGuestIds();
    }
}
