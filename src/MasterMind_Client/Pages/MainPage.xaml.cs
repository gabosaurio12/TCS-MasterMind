using MasterMind_Client.ViewModels.Pages;
using System.Windows.Controls;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml.
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
