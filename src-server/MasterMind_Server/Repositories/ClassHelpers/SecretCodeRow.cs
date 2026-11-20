namespace MasterMind_Server.Repositories.ClassHelpers
{
    internal class SecretCodeRow
    {
        public string FirstColor { get; set; }

        public string SecondColor { get; set; }

        public string ThirdColor { get; set; }

        public string FourthColor { get; set; }

        public string[] ToArray()
        {
            return new[] { FirstColor, SecondColor, ThirdColor, FourthColor };
        }
    }
}
