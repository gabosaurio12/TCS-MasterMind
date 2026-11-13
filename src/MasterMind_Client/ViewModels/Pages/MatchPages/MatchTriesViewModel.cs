using MasterMind_Client.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MasterMind_Client.ViewModels.Pages.MatchPages
{
    public class MatchTriesViewModel : ViewModelBase
    {
        private string remainingTriesText = "12";
        private string selectedColor = "Blue";

        public string RemainingTriesText
        {
            get => remainingTriesText;
            set => SetProperty(ref remainingTriesText, value);
        }

        public string SelectedColor
        {
            get => selectedColor;
            set
            {
                if (!Equals(selectedColor, value))
                {
                    selectedColor = value;
                    SetProperty(ref selectedColor, value);
                    OnPropertyChanged(nameof(IsBlueSelected));
                    OnPropertyChanged(nameof(IsRedSelected));
                    OnPropertyChanged(nameof(IsYellowSelected));
                    OnPropertyChanged(nameof(IsGreenSelected));
                    OnPropertyChanged(nameof(IsWhiteSelected));
                    OnPropertyChanged(nameof(IsOrangeSelected));
                }
            }
        }

        public bool IsBlueSelected => SelectedColor == "Blue";
        public bool IsRedSelected => SelectedColor == "Red";
        public bool IsYellowSelected => SelectedColor == "Yellow";
        public bool IsGreenSelected => SelectedColor == "Green";
        public bool IsWhiteSelected => SelectedColor == "White";
        public bool IsOrangeSelected => SelectedColor == "Orange";

        public ObservableCollection<MatchRowViewModel> TriesRows { get; } = new ObservableCollection<MatchRowViewModel>();

        public ICommand SelectColorCommand { get; }
        public ICommand TryCommand { get; }

        public MatchTriesViewModel()
        {
            SelectColorCommand = new RelayCommand(param => {
                if (param is string color)
                {
                    SelectedColor = color;
                }
            });

            TryCommand = new RelayCommand(_ => {
                // Placeholder action for submitting a try
            });

            for (int i = 0; i < 6; i++)
            {
                TriesRows.Add(new MatchRowViewModel(this));
            }
        }

        public void PlacePin(MatchPinSlotViewModel slot)
        {
            if (slot != null)
            {
                slot.PinColor = SelectedColor;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            SetProperty(ref selectedColor, selectedColor, propertyName);
        }
    }

    public class MatchRowViewModel : ViewModelBase
    {
        private readonly MatchTriesViewModel parentViewModel;

        public ObservableCollection<MatchPinSlotViewModel> PinSlots { get; } = new ObservableCollection<MatchPinSlotViewModel>();
        public ObservableCollection<MatchClueSlotViewModel> ClueSlots { get; } = new ObservableCollection<MatchClueSlotViewModel>();

        public MatchRowViewModel(MatchTriesViewModel parent)
        {
            parentViewModel = parent;
            for (int i = 0; i < 4; i++)
            {
                PinSlots.Add(new MatchPinSlotViewModel(parentViewModel));
            }
            for (int i = 0; i < 4; i++)
            {
                ClueSlots.Add(new MatchClueSlotViewModel());
            }
        }
    }

    public class MatchPinSlotViewModel : ViewModelBase
    {
        private readonly MatchTriesViewModel parentViewModel;
        private string pinColor = string.Empty;

        public string PinColor
        {
            get => pinColor;
            set
            {
                if (!Equals(pinColor, value))
                {
                    pinColor = value;
                    SetProperty(ref pinColor, value);
                    SetProperty(ref pinColor, value, nameof(HasPin));
                    SetProperty(ref pinColor, value, nameof(PinImageSource));
                }
            }
        }

        public bool HasPin => !string.IsNullOrEmpty(PinColor);

        public object PinImageSource
        {
            get
            {
                switch (PinColor)
                {
                    case "Blue":
                        return App.Current.Resources["BluePinImage"];
                    case "Red":
                        return App.Current.Resources["RedPinImage"];
                    case "Yellow":
                        return App.Current.Resources["YellowPinImage"];
                    case "Green":
                        return App.Current.Resources["GreenPinImage"];
                    case "White":
                        return App.Current.Resources["WhitePinImage"];
                    case "Orange":
                        return App.Current.Resources["OrangePinImage"];
                    default:
                        return null;
                }
            }
        }

        public ICommand PlacePinCommand { get; }

        public MatchPinSlotViewModel(MatchTriesViewModel parent)
        {
            parentViewModel = parent;
            PlacePinCommand = new RelayCommand(_ => parentViewModel.PlacePin(this));
        }
    }

    public class MatchClueSlotViewModel : ViewModelBase
    {
        private string clueType = string.Empty;

        public string ClueType
        {
            get => clueType;
            set
            {
                if (!Equals(clueType, value))
                {
                    clueType = value;
                    SetProperty(ref clueType, value);
                    SetProperty(ref clueType, value, nameof(HasClue));
                    SetProperty(ref clueType, value, nameof(ClueImageSource));
                }
            }
        }

        public bool HasClue => !string.IsNullOrEmpty(ClueType);

        public object ClueImageSource
        {
            get
            {
                switch (ClueType)
                {
                    case "White":
                        return App.Current.Resources["WhiteCluePinImage"];
                    case "Black":
                        return App.Current.Resources["BlackCluePinImage"];
                    default:
                        return null;
                }
            }
        }
    }
}
