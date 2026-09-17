using MasterMind_Client.Assets;
using MasterMind_Client.TempData;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private TempPlayer GetFormsData()
        {
            return new TempPlayer
            {
                Username = UsernameTxt.Text,
                Password = PasswordTxt.Password
            };
        }

        private bool ValidateFormsData(TempPlayer player)
        {
            if (string.IsNullOrWhiteSpace(player.Username) || string.IsNullOrWhiteSpace(player.Password))
            {
                MessageBox.Show(Properties.Resources.ErrorModal_InvalidInput, Properties.Resources.ErrorModal_InvalidInputTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!TempData.TempData.Players.Exists(p => p.Username == player.Username && p.Password == player.Password))
            {
                MessageBox.Show(Properties.Resources.ErrorModal_WrongCredentials, Properties.Resources.ErrorModal_InvalidInputTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var player = GetFormsData();
            if (ValidateFormsData(player))
            {
                NavigationService.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
            }
        }

        private void SignupLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/SignupPage.xaml", UriKind.Relative));
        }


        private void LanguageBtn_Click(object sender, RoutedEventArgs e)
        {
            UtilsUI.ChangeLanguage(NavigationService);
        }
    }
}
