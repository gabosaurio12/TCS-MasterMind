using System;
using System.Windows.Navigation;

namespace MasterMind_Client.ViewModels.Services
{
    public class NavigationServiceAdapter : INavigationService
    {
        private NavigationService navigationService;

        public void SetNavigationService(NavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public void Navigate(Uri uri)
        {
            navigationService?.Navigate(uri);
        }

        public void Refresh()
        {
            navigationService?.Refresh();
        }
    }
}
