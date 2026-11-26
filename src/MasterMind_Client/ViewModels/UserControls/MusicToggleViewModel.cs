using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class MusicToggleViewModel : ViewModelBase
    {
        private readonly IMusicService musicService;
        private bool isMuted;

        public MusicToggleViewModel(IMusicService musicService)
        {
            this.musicService = musicService;
            isMuted = musicService.IsMuted;
            ToggleCommand = new RelayCommand(_ => Toggle());
        }

        public RelayCommand ToggleCommand { get; }

        public bool IsMuted
        {
            get => isMuted;
            private set => SetProperty(ref isMuted, value);
        }

        private void Toggle()
        {
            musicService.ToggleMute();
            IsMuted = musicService.IsMuted;
        }
    }
}
