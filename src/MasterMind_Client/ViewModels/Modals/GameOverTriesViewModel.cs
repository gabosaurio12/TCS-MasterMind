using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.Modals
{
    public class GameOverTriesViewModel : ModalViewModelBase
    {
        public GameOverTriesViewModel()
        {
            ContinueCommand = new RelayCommand(_ => RequestClose());
        }

        public RelayCommand ContinueCommand { get; }
    }
}
