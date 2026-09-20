using MasterMind_Client.Assets;
using MasterMind_Client.Modals;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/ProfilePage.xaml", UriKind.Relative));
        }

        private void FriendsBtn_Click(object sender, RoutedEventArgs e)
        {
            new FriendsModal().Show();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LanguageBtn_Click(object sender, RoutedEventArgs e)
        {
            UtilsUI.ChangeLanguage(NavigationService);
        }
    }
}
