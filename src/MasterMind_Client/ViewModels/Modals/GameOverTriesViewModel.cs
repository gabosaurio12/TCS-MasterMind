using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.Modals
{
    public class GameOverTriesViewModel : ModalViewModelBase
    {
        public GameOverTriesViewModel(string title)
        {
            Title = title;
            ContinueCommand = new RelayCommand(_ => RequestClose());
        }

        public string Title { get; }

        public RelayCommand ContinueCommand { get; }
    }
}
