using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class SoundToggleViewModel : ViewModelBase
    {
        private readonly ISoundEffectService soundEffectService;
        private bool isMuted;

        public SoundToggleViewModel(ISoundEffectService soundEffectService)
        {
            this.soundEffectService = soundEffectService;
            isMuted = soundEffectService.IsMuted;
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
            soundEffectService.ToggleMute();
            IsMuted = soundEffectService.IsMuted;
        }
    }
}
