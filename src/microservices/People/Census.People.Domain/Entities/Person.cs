using Census.People.Domain.Values;
using System;

namespace Census.People.Domain.Entities
{
    public class Person
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Sex { get; set; }

        public string Race { get; set; }

        public string Education { get; set; }

        public Address Address { get; set; }

        public string FatherId { get; set; }

        public string MotherId { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Person person &&
                   Id == person.Id &&
                   Name == person.Name &&
                   Sex == person.Sex &&
                   Race == person.Race &&
                   Education == person.Education &&
                   FatherId == person.FatherId &&
                   MotherId == person.MotherId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Sex, Race, Education, FatherId, MotherId);
        }
    }
}
