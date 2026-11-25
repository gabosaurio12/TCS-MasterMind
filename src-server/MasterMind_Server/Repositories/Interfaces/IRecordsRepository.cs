using System.Collections.Generic;

namespace MasterMind_Server.Repositories.Interfaces
{
    interface IRecordsRepository
    {
        void SaveTriesRecords(IReadOnlyDictionary<string, int> bestTriesByUsername);
    }
}
