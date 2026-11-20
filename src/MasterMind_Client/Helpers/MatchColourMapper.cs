using MasterMind_Client.MatchService;
using MasterMind_Client.ViewModels.Pages.MatchPages;
using System.Collections.Generic;
using System.Linq;

namespace MasterMind_Client.Helpers
{
    public static class MatchColourMapper
    {
        private const string BlackClue = "Black";
        private const string WhiteClue = "White";

        public static CodeTry ToCodeTry(IEnumerable<PinColour> code)
        {
            return new CodeTry { Code = code.Select(colour => colour.ToString()).ToArray() };
        }

        public static ClueKind ToClueKind(string clue)
        {
            switch (clue)
            {
                case BlackClue:
                    return ClueKind.Black;
                case WhiteClue:
                    return ClueKind.White;
                default:
                    return ClueKind.None;
            }
        }
    }
}
