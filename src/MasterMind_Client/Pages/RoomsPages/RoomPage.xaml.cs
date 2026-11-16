using MasterMind_Client.ViewModels.Pages.RoomsPages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MasterMind_Client.Pages.RoomsPages
{
    /// <summary>
    /// Interaction logic for RoomPage.xaml.
    /// </summary>
    public partial class RoomPage : Page
    {
        public RoomPage()
        {
            InitializeComponent();
            Loaded += RoomPage_Loaded;
            Unloaded += RoomPage_Unloaded;
            ChatTxt.TargetUpdated += ChatTxt_TargetUpdated;
        }

        private void ChatTxt_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            ChatScroll.ScrollToEnd();
        }


        private void RoomPage_Loaded(object sender, RoutedEventArgs e)
        {
            var uri = NavigationService?.CurrentSource;
            if (uri != null)
            {
                DataContext = new RoomPageViewModel(uri, AppServices.NavigationService, AppServices.DialogService);
            }
        }

        private void RoomPage_Unloaded(object sender, RoutedEventArgs e)
        {
            (DataContext as IDisposable)?.Dispose();
        }
    }
}
