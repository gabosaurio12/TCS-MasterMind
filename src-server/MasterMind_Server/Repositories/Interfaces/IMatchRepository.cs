using MasterMind_Server.Repositories.ClassHelpers;

namespace MasterMind_Server.Repositories.Interfaces
{
    interface IMatchRepository
    {
        MatchParticipation GetParticipation(string username);

        void SaveSecretCode(int roomId, string[] code);

        string[] GetSecretCode(int roomId);

        bool RegisterTry(MatchParticipation participation);
    }
}
