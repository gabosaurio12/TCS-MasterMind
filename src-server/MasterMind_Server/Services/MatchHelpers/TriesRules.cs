namespace MasterMind_Server.Services.MatchHelpers
{
    internal static class TriesRules
    {
        private const int EasyMaxTries = 10;
        private const int NormalMaxTries = 8;
        private const int HardMaxTries = 5;
        private const int EnigmaMaxTries = 3;

        public static int GetMaxTries(string difficulty)
        {
            switch (difficulty)
            {
                case "Easy":
                    return EasyMaxTries;
                case "Hard":
                    return HardMaxTries;
                case "Enigma":
                    return EnigmaMaxTries;
                default:
                    return NormalMaxTries;
            }
        }
    }
}
