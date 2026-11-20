using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.Pages.MatchPages
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
