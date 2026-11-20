namespace MasterMind_Client.ViewModels.Pages.MatchPages
{
    public static class MatchRules
    {
        public const int CodeLength = 4;
        public const int BoardRowCount = 6;

        private const int EasyMaxTries = 10;
        private const int NormalMaxTries = 8;
        private const int HardMaxTries = 5;
        private const int EnigmaMaxTries = 3;

        public static int GetMaxTries(string difficulty)
        {
            switch (difficulty)
            {
                case "Normal":
                    return NormalMaxTries;
                case "Hard":
                    return HardMaxTries;
                case "Enigma":
                    return EnigmaMaxTries;
                default:
                    return EasyMaxTries;
            }
        }
    }
}
