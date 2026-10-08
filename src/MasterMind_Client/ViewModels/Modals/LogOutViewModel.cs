using System.Windows;
using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.Modals
{
    public class LogOutViewModel : ViewModelBase
    {
        public LogOutViewModel()
        {
            QuitCommand = new RelayCommand(_ => Application.Current.Shutdown());
            LogOutCommand = new RelayCommand(_ => LogOut());
        }

        public RelayCommand QuitCommand { get; }

        public RelayCommand LogOutCommand { get; }

        private void LogOut()
        {
            Properties.Settings.Default.RememberLogin = false;
            Properties.Settings.Default.SavedUsername = "";
            Properties.Settings.Default.Save();

            Application.Current.Shutdown();
        }
    }
}
