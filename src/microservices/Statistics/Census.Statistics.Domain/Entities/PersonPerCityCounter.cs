using System.Collections.Generic;

namespace Census.Statistics.Domain.Entities
{
    public class PersonPerCityCounter
    {
        public string Id { get; set; }

        public string City { get; set; }

        public long Count { get; set; }

        public Dictionary<string, PersonNameCounter> PersonNameCounters { get; set; }

        public PersonPerCityCounter()
        {
            PersonNameCounters = new Dictionary<string, PersonNameCounter>();
        }
    }
}
