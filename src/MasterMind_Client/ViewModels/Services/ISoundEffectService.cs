namespace MasterMind_Client.ViewModels.Services
{
    /// <summary>
    /// Plays short interaction sounds and exposes the persisted mute state of the effects.
    /// </summary>
    public interface ISoundEffectService
    {
        /// <summary>
        /// Gets a value indicating whether the sound effects are muted.
        /// </summary>
        bool IsMuted { get; }

        /// <summary>
        /// Plays an effect without interrupting the ones already playing.
        /// </summary>
        /// <param name="effect">The effect to play.</param>
        void Play(SoundEffect effect);

        /// <summary>
        /// Switches the mute state of the effects and persists it.
        /// </summary>
        void ToggleMute();
    }
}
