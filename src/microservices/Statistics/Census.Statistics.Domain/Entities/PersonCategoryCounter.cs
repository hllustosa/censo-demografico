using System.Collections.Generic;

namespace Census.Statistics.Domain.Entities
{
    public class PersonCategoryCounter
    {
        public string Id { get; set; }

        public string Race { get; set; }

        public string SchoolLevel { get; set; }

        public string Sex { get; set; }

        public long Count { get; set; }

        public Dictionary<string, PersonNameCounter> PersonNameCounters { get; set; }

        public PersonCategoryCounter()
        {
            PersonNameCounters = new Dictionary<string, PersonNameCounter>();
        }
    }
}
