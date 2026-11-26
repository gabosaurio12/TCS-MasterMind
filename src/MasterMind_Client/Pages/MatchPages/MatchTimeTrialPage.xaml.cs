using MasterMind_Client.ViewModels.Pages.MatchPages.MatchTimeTrial;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MasterMind_Client.Pages.MatchPages
{
    /// <summary>
    /// Interaction logic for MatchTimeTrialPage.xaml.
    /// </summary>
    public partial class MatchTimeTrialPage : Page
    {
        public MatchTimeTrialPage()
        {
            InitializeComponent();
            Loaded += MatchTimeTrialPage_Loaded;
            Unloaded += MatchTimeTrialPage_Unloaded;
        }

        private async void MatchTimeTrialPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            var uri = NavigationService?.CurrentSource;
            if (uri == null || DataContext != null)
            {
                return;
            }

            var viewModel = new MatchTimeTrialViewModel(uri, AppServices.NavigationService, AppServices.DialogService);
            DataContext = viewModel;
            await viewModel.InitializeAsync();
        }

        private void MatchTimeTrialPage_Unloaded(object sender, RoutedEventArgs e)
        {
            (DataContext as IDisposable)?.Dispose();
            DataContext = null;
        }
    }
}
