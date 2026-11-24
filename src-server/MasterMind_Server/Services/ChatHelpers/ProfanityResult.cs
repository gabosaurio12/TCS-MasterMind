namespace MasterMind_Server.Services.ChatHelpers
{
    internal sealed class ProfanityResult
    {
        public ProfanityResult(string censoredText, bool hasBannedWords)
        {
            CensoredText = censoredText;
            HasBannedWords = hasBannedWords;
        }

        public string CensoredText { get; }

        public bool HasBannedWords { get; }
    }
}
