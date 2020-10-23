using System.Collections.Generic;

namespace Census.People.Domain.Values
{
    public class Education
    {
        public static readonly string ILLITERATE = "Analfabeto(a)";

        public static readonly string LITERATE = "Alfabetizado(a)";

        public static readonly string ELEMENTARY = "Ensino Fundamental";

        public static readonly string HIGH_SCHOOL = "Ensino Médio";

        public static readonly string COLLEGE = "Ensino Superior";

        public static readonly string GRAD_SCHOOL = "Pós-Graduação";

        public static IEnumerable<string> Values()
        {
            return new List<string>()
            {
                ILLITERATE,
                LITERATE,
                ELEMENTARY,
                HIGH_SCHOOL,
                COLLEGE,
                GRAD_SCHOOL
            };
        }
    }
}
