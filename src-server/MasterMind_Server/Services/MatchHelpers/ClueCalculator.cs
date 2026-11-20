using MasterMind_Server.Repositories.Enums;
using System.Collections.Generic;
using System.Linq;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal static class ClueCalculator
    {
        private const int CodeLength = 4;

        public static string[] Calculate(string[] secretCode, string[] guess)
        {
            var clues = Enumerable.Repeat(ClueType.None.ToString(), CodeLength).ToArray();
            var unmatchedColours = MarkExactMatches(secretCode, guess, clues);
            MarkColourMatches(guess, clues, unmatchedColours);
            return clues;
        }

        public static bool IsExactMatch(string[] clues)
        {
            return clues.All(clue => clue == ClueType.Black.ToString());
        }

        private static Dictionary<string, int> MarkExactMatches(string[] secretCode, string[] guess, string[] clues)
        {
            var unmatchedColours = new Dictionary<string, int>();
            for (int i = 0; i < CodeLength; i++)
            {
                if (secretCode[i] == guess[i])
                {
                    clues[i] = ClueType.Black.ToString();
                    continue;
                }

                unmatchedColours.TryGetValue(secretCode[i], out var count);
                unmatchedColours[secretCode[i]] = count + 1;
            }

            return unmatchedColours;
        }

        private static void MarkColourMatches(string[] guess, string[] clues, Dictionary<string, int> unmatchedColours)
        {
            for (int i = 0; i < CodeLength; i++)
            {
                if (clues[i] == ClueType.Black.ToString())
                {
                    continue;
                }

                if (unmatchedColours.TryGetValue(guess[i], out var count) && count > 0)
                {
                    clues[i] = ClueType.White.ToString();
                    unmatchedColours[guess[i]] = count - 1;
                }
            }
        }
    }
}
