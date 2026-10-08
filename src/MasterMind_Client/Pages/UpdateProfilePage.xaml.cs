using MasterMind_Client.ViewModels.Pages;
using System.Windows;
using System.Windows.Controls;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para UpdateProfilePage.xaml
    /// </summary>
    public partial class UpdateProfilePage : Page
    {
        public UpdateProfilePage()
        {
            InitializeComponent();
            DataContext = new UpdateProfilePageViewModel(AppServices.NavigationService, AppServices.DialogService);
        }
    }
}
