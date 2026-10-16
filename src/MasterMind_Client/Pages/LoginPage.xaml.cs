using MasterMind_Client.ViewModels.Pages;
using System.Windows;
using System.Windows.Controls;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml.
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            DataContext = new LoginPageViewModel(AppServices.NavigationService, AppServices.DialogService);
        }

        private void PasswordTxt_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((LoginPageViewModel)DataContext).Password = PasswordTxt.Password;
        }

        private void SignupLink_Click(object sender, RoutedEventArgs e)
        {
            ((LoginPageViewModel)DataContext).SignupCommand.Execute(null);
        }
    }
}
