using MasterMind_Client.ViewModels.Base;
using System;
using System.Threading.Tasks;

namespace MasterMind_Client.ViewModels.Modals
{
    public class MatchMenuViewModel : ModalViewModelBase
    {
        public MatchMenuViewModel(Func<Task> exitConfirmed)
        {
            ResumeCommand = new RelayCommand(_ => RequestClose());
            ExitCommand = new RelayCommand(async _ =>
            {
                RequestClose();
                await exitConfirmed();
            });
        }

        public RelayCommand ResumeCommand { get; }

        public RelayCommand ExitCommand { get; }
    }
}
