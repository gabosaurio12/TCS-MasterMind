using MasterMind_Client.ViewModels.Services;

namespace MasterMind_Client
{
    public static class AppServices
    {
        public static INavigationService NavigationService { get; set; }

        public static IDialogService DialogService { get; set; }

        public static IMusicService MusicService { get; set; }

        public static ISoundEffectService SoundEffectService { get; set; }
    }
}
