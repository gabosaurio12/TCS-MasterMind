using System;

namespace MasterMind_Client.ViewModels.Services
{
    public interface INavigationService
    {
        void Navigate(Uri uri);

        void Refresh();
    }
}
