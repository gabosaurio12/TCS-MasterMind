using MasterMind_Client.MatchService;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers
{
    public class MatchSession
    {
        public MatchSession(MatchServiceClient client, MatchSnapshot snapshot)
        {
            Client = client;
            Snapshot = snapshot;
        }

        public MatchServiceClient Client { get; }

        public MatchSnapshot Snapshot { get; }
    }
}
