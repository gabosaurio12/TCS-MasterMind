using MasterMind_Server.Repositories.Enums;
using System;
using System.Linq;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal static class SecretCodeGenerator
    {
        private const int ByteRange = 256;
        private const int CodeLength = 4;
        private static readonly string[] colours = Enum.GetNames(typeof(Colours));

        public static string[] Generate()
        {
            int min = 0;
            int max = 500;
            int[] numCode = new int[CodeLength];

            for (int i = 0; i < CodeLength; i++)
            {
                int number = new Random().Next(min, max);
                numCode[i] = GetDigit(number);
            }

            string[] code = numCode.Select(n => colours[n]).ToArray();
            return code;
        }

        private static int GetDigit(int number)
        {
            while ((number / 10) > 0)
            {
                number /= 10;
            }

            return number;
        }
    }
}
