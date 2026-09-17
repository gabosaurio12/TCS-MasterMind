using MasterMind_Client.Assets;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para SignupPage.xaml
    /// </summary>
    public partial class SignupPage : Page
    {
        public SignupPage()
        {
            InitializeComponent();
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/LoginPage.xaml", UriKind.Relative));
        }

        private void LanguageBtn_Click(object sender, RoutedEventArgs e)
        {
            UtilsUI.ChangeLanguage(NavigationService);
        }
    }
}
