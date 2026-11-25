using System;
using System.Text.RegularExpressions;

namespace MasterMind_Client.Helpers
{
    public static class UsernameValidator
    {
        private const int RegexTimeoutMilliseconds = 500;

        private static readonly Regex WhitespaceRegex = new Regex(
            @"\s",
            RegexOptions.None,
            TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds));

        public static bool HasWhitespace(string username)
        {
            return WhitespaceRegex.IsMatch(username);
        }
    }
}
