using System;
using System.Collections.Generic;
using System.Text;

namespace Census.Statistics.Domain.Entities
{
    public class PersonCategoryFilter
    {
        public string _id { get; set; }

        public string Name { get; set; }

        public string Sex { get; set; }

        public string Race { get; set; }

        public string SchoolLevel { get; set; }
    }
}
