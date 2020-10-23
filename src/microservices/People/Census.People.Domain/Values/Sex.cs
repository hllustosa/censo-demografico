using System.Collections.Generic;

namespace Census.People.Domain.Values
{
    public class Sex
    {
        public static readonly char MALE = 'M';

        public static readonly char FEMALE = 'F';

        public static readonly char UNDEFINED = 'I';

        public static IEnumerable<char> Values()
        {
            return new List<char>()
            {
                MALE,
                FEMALE,
                UNDEFINED
            };
        }
    }
}
