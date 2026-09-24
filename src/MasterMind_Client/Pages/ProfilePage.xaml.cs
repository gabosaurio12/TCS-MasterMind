using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            SetPlayerInfo();
        }

        private void SetPlayerInfo()
        {
            PlayerUsernameLbl.Content = CurrentPlayer.Instance.Username;
            PlayerEmailLbl.Content = CurrentPlayer.Instance.Email;
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

        private void ReportBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/UpdateProfilePage.xaml", UriKind.Relative));
        }

        private void ChangePasswordLink_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
        }
    }
}
