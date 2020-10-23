using System;
using System.Collections.Generic;
using System.Text;

namespace Census.People.Domain.Entities
{
    public class PageResult<T>
    {
        public IEnumerable<T> Items { get; set; }

        public int Page { get; set; }

        public int TotalItems { get; set; }
    }
}
