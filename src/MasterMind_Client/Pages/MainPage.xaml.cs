using MasterMind_Client.ViewModels.Pages;
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
            DataContext = new MainPageViewModel(AppServices.NavigationService, AppServices.DialogService);
        }
    }
}
