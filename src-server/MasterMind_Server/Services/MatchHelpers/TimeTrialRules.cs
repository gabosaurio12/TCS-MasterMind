namespace MasterMind_Server.Services.MatchHelpers
{
    internal static class TimeTrialRules
    {
        public const string TimeTrialGameMode = "TimeTrial";

        private const int EasySeconds = 60;
        private const int NormalSeconds = 30;
        private const int HardSeconds = 15;
        private const int EnigmaSeconds = 10;

        public static int GetRoundSeconds(string difficulty)
        {
            switch (difficulty)
            {
                case "Easy":
                    return EasySeconds;
                case "Hard":
                    return HardSeconds;
                case "Enigma":
                    return EnigmaSeconds;
                default:
                    return NormalSeconds;
            }
        }

        public static bool IsTimeTrial(string gameMode)
        {
            return gameMode == TimeTrialGameMode;
        }
    }
}
