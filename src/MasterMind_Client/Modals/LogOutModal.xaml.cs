using System.Windows;

namespace MasterMind_Client.Modals
{
    /// <summary>
    /// Lógica de interacción para LogOutModal.xaml
    /// </summary>
    public partial class LogOutModal : Window
    {
        public LogOutModal()
        {
            InitializeComponent();
        }

        private void QuitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LogOutBtn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RememberLogin = false;
            Properties.Settings.Default.SavedUsername = "";
            Properties.Settings.Default.Save();

            Application.Current.Shutdown();
        }
    }
}
