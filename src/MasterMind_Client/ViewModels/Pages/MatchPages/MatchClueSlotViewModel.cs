using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.Pages.MatchPages
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
