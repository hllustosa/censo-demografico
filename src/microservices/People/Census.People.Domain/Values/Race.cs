using System.Collections.Generic;

namespace Census.People.Domain.Values
{
    public class Race
    {
        public static readonly string CAUCASIAN = "Branco(a)";

        public static readonly string BROWN = "Pardo(a)";

        public static readonly string BLACK = "Negro(a)";

        public static readonly string ASIAN = "Amarelo(a)";

        public static readonly string NATIVE = "Indígena";

        public static readonly string NA = "Não Informada";

        public static IEnumerable<string> Values()
        {
            return new List<string>()
            {
                CAUCASIAN,
                BROWN,
                BLACK,
                ASIAN,
                NATIVE,
                NA
            };
        }
    }
}
