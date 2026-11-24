using MasterMind_Client.ViewModels.Pages.MatchPages.MatchTries;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MasterMind_Client.Pages.MatchPages
{
    /// <summary>
    /// Interaction logic for MatchTriesPage.xaml.
    /// </summary>
    public partial class MatchTriesPage : Page
    {
        public MatchTriesPage()
        {
            InitializeComponent();
            Loaded += MatchTriesPage_Loaded;
            Unloaded += MatchTriesPage_Unloaded;
        }

        private async void MatchTriesPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            var uri = NavigationService?.CurrentSource;
            if (uri == null || DataContext != null)
            {
                return;
            }

            var viewModel = new MatchTriesViewModel(uri, AppServices.NavigationService, AppServices.DialogService);
            DataContext = viewModel;
            await viewModel.InitializeAsync();
        }

        private void MatchTriesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            (DataContext as IDisposable)?.Dispose();
            DataContext = null;
        }
    }
}
