namespace risk.control.system.Helpers
{
    public class NameGenerator
    {
        private static readonly string[] firstNames = { "John", "Paul", "Ringo", "George", "Laura", "Stephaney" };
        private static readonly string[] lastNames = { "Lennon", "McCartney", "Starr", "Harrison", "Blanc" };

        public static string GenerateName()
        {
            var random = new Random();
            string firstName = firstNames[random.Next(0, firstNames.Length)];
            string lastName = lastNames[random.Next(0, lastNames.Length)];

            return $"{firstName} {lastName}";
        }
    }
}