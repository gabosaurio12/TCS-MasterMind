using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchTries.Helpers;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchTries.ViewModels
{
    public class PaletteColourViewModel : ViewModelBase
    {
        private bool isSelected;

        public PaletteColourViewModel(PinColour colour)
        {
            Colour = colour;
        }

        public PinColour Colour { get; }

        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }
    }
}
