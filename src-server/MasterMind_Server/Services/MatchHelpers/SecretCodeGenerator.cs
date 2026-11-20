using MasterMind_Server.Repositories.Enums;
using System;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal static class SecretCodeGenerator
    {
        private const int CodeLength = 4;
        private static readonly string[] colours = Enum.GetNames(typeof(Colours));
        private static readonly Random random = new Random();

        public static string[] Generate()
        {
            string[] code = new string[CodeLength];

            for (int i = 0; i < CodeLength; i++)
            {
                int colourIndex = random.Next(colours.Length);
                code[i] = colours[colourIndex];
            }

            return code;
        }
    }
}
