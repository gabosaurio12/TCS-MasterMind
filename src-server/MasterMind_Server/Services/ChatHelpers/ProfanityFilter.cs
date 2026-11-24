using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MasterMind_Server.Services.ChatHelpers
{
    internal static class ProfanityFilter
    {
        private const char CensorshipCharacter = '*';
        private const int RegexTimeoutMilliseconds = 500;
        private const string WordSuffixes = "s|es|y|er|ers|ing|ed|ito|ita|azo";

        private static readonly string[] bannedWords =
        {
            "pendejo", "pendeja", "mierda", "puto", "puta", "cabron", "cabrona", "chingar", "chingada",
            "chingado", "chingadera", "verga", "culero", "culera", "idiota", "imbecil", "estupido",
            "estupida", "maricon", "marica", "joto", "zorra", "carajo", "gilipollas", "hijueputa",
            "malparido", "mamada", "mamon", "pinche", "huevon", "tarado", "fuck", "motherfucker",
            "shit", "bullshit", "bitch", "asshole", "bastard", "dick", "cunt", "dumbass", "jackass",
            "whore", "slut", "piss", "wanker", "twat", "prick", "douche", "faggot", "nigger"
        };

        private static readonly Dictionary<char, char> leetSubstitutions = new Dictionary<char, char>
        {
            { '0', 'o' }, { '1', 'i' }, { '3', 'e' }, { '4', 'a' }, { '5', 's' }, { '@', 'a' }, { '$', 's' }
        };

        private static readonly Regex bannedWordsRegex = BuildBannedWordsRegex();

        public static ProfanityResult Censor(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new ProfanityResult(string.Empty, false);
            }

            var censoredText = new StringBuilder(text);
            var matches = bannedWordsRegex.Matches(Normalize(text)).Cast<Match>().ToList();
            matches.ForEach(match => MaskRange(censoredText, match.Index, match.Length));

            return new ProfanityResult(censoredText.ToString(), matches.Count > 0);
        }

        private static void MaskRange(StringBuilder text, int start, int length)
        {
            text.Remove(start, length).Insert(start, new string(CensorshipCharacter, length));
        }

        /*
        Normalization must keep the text length: the match positions found in the
        normalized text are used to mask the very same range in the original text.
        */
        private static string Normalize(string text)
        {
            return new string(text.Select(NormalizeCharacter).ToArray());
        }

        private static char NormalizeCharacter(char character)
        {
            var lowered = char.ToLowerInvariant(character);
            char substitution;
            if (leetSubstitutions.TryGetValue(lowered, out substitution))
            {
                return substitution;
            }

            var decomposed = lowered.ToString().Normalize(NormalizationForm.FormD);
            return decomposed[0];
        }

        private static Regex BuildBannedWordsRegex()
        {
            var words = string.Join("|", bannedWords.Select(Regex.Escape));
            return new Regex(
                $"(?<![a-z])({words})({WordSuffixes})?(?![a-z])",
                RegexOptions.Compiled | RegexOptions.CultureInvariant,
                TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds));
        }
    }
}
