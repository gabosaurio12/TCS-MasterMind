using System.Windows;
using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.Modals
{
    public class LogOutViewModel : ViewModelBase
    {

        private bool isLogOutVisible;
        public bool IsLogOutVisible
        {
            get => isLogOutVisible;
            set
            {
                isLogOutVisible = value;
                SetProperty(ref isLogOutVisible, value);
            }
        }

        public RelayCommand QuitCommand { get; }
        public RelayCommand LogOutCommand { get; }

        public LogOutViewModel()
        {
            QuitCommand = new RelayCommand(_ => Application.Current.Shutdown());
            LogOutCommand = new RelayCommand(_ => LogOut());
            IsLogOutVisible = Properties.Settings.Default.RememberLogin;
        }

        private void LogOut()
        {
            Properties.Settings.Default.RememberLogin = false;
            Properties.Settings.Default.SavedUsername = "";
            Properties.Settings.Default.Save();

            Application.Current.Shutdown();
        }
    }
}
