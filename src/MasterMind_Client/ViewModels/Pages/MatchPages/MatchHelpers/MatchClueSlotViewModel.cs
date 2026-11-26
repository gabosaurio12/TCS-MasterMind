using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers
{
    public class MatchClueSlotViewModel : ViewModelBase
    {
        private ClueColour kind;

        public ClueColour Kind
        {
            get => kind;
            set => SetProperty(ref kind, value);
        }
    }
}
