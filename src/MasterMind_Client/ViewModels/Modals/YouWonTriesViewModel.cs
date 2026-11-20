using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Pages.MatchPages;
using System.Collections.Generic;

namespace MasterMind_Client.ViewModels.Modals
{
    public class YouWonTriesViewModel : ModalViewModelBase
    {
        public YouWonTriesViewModel(IReadOnlyList<PinColour> secretCode)
        {
            SecretCode = secretCode;
            ContinueCommand = new RelayCommand(_ => RequestClose());
        }

        public IReadOnlyList<PinColour> SecretCode { get; }

        public RelayCommand ContinueCommand { get; }
    }
}
