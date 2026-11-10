using System;
using System.Text.RegularExpressions;

namespace MasterMind_Client.Helpers
{
    public static class PasswordValidator
    {
        private static readonly Regex PasswordRegex = new Regex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            RegexOptions.None,
            TimeSpan.FromMilliseconds(100));

        public static bool IsValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            return PasswordRegex.IsMatch(password);
        }
    }
}
