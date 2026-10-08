using MasterMind_Client.ViewModels.Pages.RoomsPages;
using System.Windows.Controls;

namespace MasterMind_Client.Pages.RoomsPages
{
    /// <summary>
    /// Lógica de interacción para RoomsPage.xaml
    /// </summary>
    public partial class RoomsPage : Page
    {
        public RoomsPage()
        {
            InitializeComponent();
            DataContext = new RoomsPageViewModel(AppServices.NavigationService, AppServices.DialogService);
        }
    }
}
