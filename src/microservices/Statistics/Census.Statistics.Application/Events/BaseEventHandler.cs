using Census.Shared.Bus.Event;
using Census.Statistics.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Census.Statistics.Application.Events
{
    public class BaseEventHandler
    {
        protected static void IncrementCategoryCounters(PersonCategoryFilter filter,
            PersonCategoryCounter personCategory)
        {
            personCategory.Count++;
            IncrementNameCounters(filter, personCategory.PersonNameCounters);
        }

        protected static void DecrementCategoryCounters(PersonCategoryFilter filter,
            PersonCategoryCounter personCategory)
        {
            personCategory.Count = Math.Max(personCategory.Count - 1, 0);
            DecrementNameCounters(filter, personCategory.PersonNameCounters);
        }

        protected static void IncrementPerCityCounters(PersonCategoryFilter filter,
            PersonPerCityCounter personCategory)
        {
            personCategory.Count++;
            IncrementNameCounters(filter, personCategory.PersonNameCounters);
        }

        protected static void DecrementPerCityCounters(PersonCategoryFilter filter,
            PersonPerCityCounter personCategory)
        {
            personCategory.Count = Math.Max(personCategory.Count - 1, 0);
            DecrementNameCounters(filter, personCategory.PersonNameCounters);
        }

        protected static void IncrementNameCounters(PersonCategoryFilter filter,
            Dictionary<string, PersonNameCounter> nameCounters)
        {
            AlterNameCounters(filter, nameCounters, 1, 1);
        }

        protected static void DecrementNameCounters(PersonCategoryFilter filter,
            Dictionary<string, PersonNameCounter> nameCounters)
        {
            AlterNameCounters(filter, nameCounters, -1, 0);
        }

        protected static void AlterNameCounters(PersonCategoryFilter filter,
           Dictionary<string, PersonNameCounter> nameCounters, int delta, int initial)
        {
            if (nameCounters.ContainsKey(filter.Name))
            {
                nameCounters[filter.Name].Count =
                    Math.Max(nameCounters[filter.Name].Count + delta, 0);
            }
            else
            {
                nameCounters[filter.Name] = new PersonNameCounter()
                {
                    Name = filter.Name,
                    Count = initial,
                };
            }
        }

        protected static PersonCategoryFilter CreateFilter(PersonDTO person)
        {
            return new PersonCategoryFilter()
            {
                Name = person.Name?.Split(null)[0],
                Sex = person.Sex,
                Race = person.Race,
                SchoolLevel = person.Education
            };
        }

        protected static PersonCategoryFilter Anonimize(PersonCategoryFilter filter)
        {
            return new PersonCategoryFilter()
            {
                Name = string.Empty,
                Race = filter.Race,
                SchoolLevel = filter.SchoolLevel,
                Sex = filter.Sex
            };
        }
    }
}
