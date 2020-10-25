using System.Collections.Generic;

namespace Census.People.Domain.Values
{
    public class Sex
    {
        public static readonly string MALE = "M";

        public static readonly string FEMALE = "F";

        public static readonly string UNDEFINED = "I";

        public static IEnumerable<string> Values()
        {
            return new List<string>()
            {
                MALE,
                FEMALE,
                UNDEFINED
            };
        }
    }
}
