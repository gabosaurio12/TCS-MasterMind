using MasterMind_Client.ViewModels.Pages.RoomsPages;
using System.Windows;
using System.Windows.Controls;

namespace MasterMind_Client.Pages.RoomsPages
{
    /// <summary>
    /// Lógica de interacción para RoomPage.xaml
    /// </summary>
    public partial class RoomPage : Page
    {
        public RoomPage()
        {
            InitializeComponent();
            Loaded += RoomPage_Loaded;
        }

        private void RoomPage_Loaded(object sender, RoutedEventArgs e)
        {
            var uri = NavigationService?.CurrentSource;
            if (uri != null)
            {
                DataContext = new RoomPageViewModel(uri, AppServices.NavigationService, AppServices.DialogService);
            }
        }
    }
}
