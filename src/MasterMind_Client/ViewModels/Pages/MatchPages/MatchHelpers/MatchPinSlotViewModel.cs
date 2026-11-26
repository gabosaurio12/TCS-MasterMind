using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers;
using System;
using System.Windows.Input;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers
{
    public class MatchPinSlotViewModel : ViewModelBase
    {
        private PinColour? colour;

        public MatchPinSlotViewModel(Action<MatchPinSlotViewModel> placePin)
        {
            PlacePinCommand = new RelayCommand(_ => placePin(this));
        }

        public PinColour? Colour
        {
            get => colour;
            set => SetProperty(ref colour, value);
        }

        public ICommand PlacePinCommand { get; }
    }
}
