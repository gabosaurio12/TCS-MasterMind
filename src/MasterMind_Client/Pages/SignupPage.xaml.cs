using MasterMind_Client.ViewModels.Pages;
using System.Windows;
using System.Windows.Controls;

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
            DataContext = new SignupPageViewModel(AppServices.NavigationService, AppServices.DialogService);
        }

        private void PasswordTxt_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((SignupPageViewModel)DataContext).Password = PasswordTxt.Password;
        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            ((SignupPageViewModel)DataContext).LoginLinkCommand.Execute(null);
        }
    }
}
