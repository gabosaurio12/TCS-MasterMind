namespace MasterMind_Client.ViewModels.Services
{
    /// <summary>
    /// Plays the looping background music and exposes the persisted mute state.
    /// </summary>
    public interface IMusicService
    {
        /// <summary>
        /// Gets a value indicating whether the music is muted.
        /// </summary>
        bool IsMuted { get; }

        /// <summary>
        /// Starts the background track in a loop.
        /// </summary>
        void Play();

        /// <summary>
        /// Switches the mute state and persists it.
        /// </summary>
        void ToggleMute();

        /// <summary>
        /// Stops the music and releases the player.
        /// </summary>
        void Stop();
    }
}
