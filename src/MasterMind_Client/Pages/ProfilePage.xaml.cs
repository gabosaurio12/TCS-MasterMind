using MasterMind_Client.ViewModels.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            DataContext = new ProfilePageViewModel(AppServices.NavigationService);
        }

        private void ReportUsernameTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            var input = ReportUsernameTxt.Text;
            if (input.Equals(Properties.Resources.ProfilePage_ReportPlayer.Trim()))
            {
                ReportUsernameTxt.Foreground = Brushes.Black;
                ReportUsernameTxt.Text = "";
            }
        }

        private void ReportUsernameTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            var input = ReportUsernameTxt.Text;
            if (input.Equals(""))
            {
                ReportUsernameTxt.Foreground = Brushes.Gray;
                ReportUsernameTxt.Text = Properties.Resources.ProfilePage_ReportPlayer.Trim();
            }
        }

        private void ChangePasswordLink_Click(object sender, RoutedEventArgs e)
        {
            ((ProfilePageViewModel)DataContext).ChangePasswordCommand.Execute(null);
        }
    }
}
