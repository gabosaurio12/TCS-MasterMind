using MasterMind_Client.ViewModels.Pages.RoomsPages;
using System.Windows.Controls;

namespace MasterMind_Client.Pages.RoomsPages
{
    /// <summary>
    /// Lógica de interacción para CreateRoomPage.xaml
    /// </summary>
    public partial class CreateRoomPage : Page
    {
        public CreateRoomPage()
        {
            InitializeComponent();
            DataContext = new CreateRoomPageViewModel(AppServices.NavigationService, AppServices.DialogService);
        }
    }
}
