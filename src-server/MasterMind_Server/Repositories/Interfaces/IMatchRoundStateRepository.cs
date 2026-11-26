using MasterMind_Server.Repositories.ClassHelpers;
using System.Collections.Generic;

namespace MasterMind_Server.Repositories.Interfaces
{
    interface IMatchRoundStateRepository
    {
        void Save(MatchRoundRecord record);

        void Delete(int roomId);

        IReadOnlyList<MatchRoundRecord> GetAll();
    }
}
