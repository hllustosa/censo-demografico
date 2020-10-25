using Census.Statistics.Domain.Entities;
using Census.Statistics.Domain.Interfaces;
using Census.Statistics.Infra.Connection;
using Census.Statistics.Infra.Repository;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Census.People.Infra.Repository
{
    public class PersonCategoryRepository : IPersonCategoryRepository
    {
        IMongoConnection MongoConnection { get; set; }

        IGuidGenerator GuidGenerator { get; set; }

        public PersonCategoryRepository(IMongoConnection mongoConnection, IGuidGenerator guidGenerator)
        {
            MongoConnection = mongoConnection;
            GuidGenerator = guidGenerator;
        }

        public async Task<List<PersonCategoryCounter>> GetPersonCategoryCounters(PersonCategoryFilter personCategoryFilter)
        {
            var collection = MongoConnection.GetPersonCategoriesCollection();
            var filter = Builders<PersonCategoryCounter>.Filter.Empty;
            filter = CreateFilterFromCategoryFilter(personCategoryFilter, filter);
            var result = await collection.FindAsync(filter);
            var list = result.ToList();

            CreateDefaultResult(list, personCategoryFilter);
            return list;
            
        }

        public async Task Save(ITransaction transaction, PersonCategoryCounter personCategoryCounter)
        {
            var session = ((MongoSession) transaction).Session;
            var collection = MongoConnection.GetPersonCategoriesCollection();
            var filter = Builders<PersonCategoryCounter>.Filter.Empty;

            personCategoryCounter.Id = await GetExistintIdAsync(personCategoryCounter);

            await collection.ReplaceOneAsync(
                filter: CreateFilterFromCategory(personCategoryCounter, filter),
                options: new ReplaceOptions { IsUpsert = true },
                replacement: personCategoryCounter,
                session: session);
        }


        private async Task<string> GetExistintIdAsync(PersonCategoryCounter personCategoryCounter)
        {
            var collection = MongoConnection.GetPersonCategoriesCollection();
            var filter = Builders<PersonCategoryCounter>.Filter.Empty;
            filter = CreateFilterFromCategory(personCategoryCounter, filter);
            var result = await collection.FindAsync(filter);
            return result.FirstOrDefault()?.Id ?? GuidGenerator.GenerateGuid();
        }

        private static void CreateDefaultResult(List<PersonCategoryCounter> list,
                PersonCategoryFilter personCategoryFilter)
        {
            if (list.Count == 0)
            {
                list.Add(new PersonCategoryCounter()
                {
                    Count = 0,
                    Race = personCategoryFilter.Race,
                    SchoolLevel = personCategoryFilter.SchoolLevel,
                    Sex = personCategoryFilter.Sex.ToString()
                });
            }
        }

        private static FilterDefinition<PersonCategoryCounter> CreateFilterFromCategory(
            PersonCategoryCounter personCategoryCounter, FilterDefinition<PersonCategoryCounter> filter)
        {
            filter = AddEqFilter(x => x.Race, personCategoryCounter.Race, filter);
            filter = AddEqFilter(x => x.Sex, personCategoryCounter.Sex, filter);
            filter = AddEqFilter(x => x.SchoolLevel, personCategoryCounter.SchoolLevel, filter);
            return filter;
        }

        private static FilterDefinition<PersonCategoryCounter> CreateFilterFromCategoryFilter(
           PersonCategoryFilter personCategoryFilter, FilterDefinition<PersonCategoryCounter> filter)
        {
            filter = AddNameFilter(personCategoryFilter, filter);
            filter = AddEqFilter(x => x.Race, personCategoryFilter.Race, filter);
            filter = AddEqFilter(x => x.Sex, personCategoryFilter.Sex, filter);
            filter = AddEqFilter(x => x.SchoolLevel, personCategoryFilter.SchoolLevel, filter);
            return filter;
        }

        private static FilterDefinition<PersonCategoryCounter> AddEqFilter(
            Expression<Func<PersonCategoryCounter, string>> field, 
            string value, 
            FilterDefinition<PersonCategoryCounter> filter)
        {
            if (!String.IsNullOrEmpty(value))
            {
                var newFilter = Builders<PersonCategoryCounter>
                    .Filter.Eq(field, value);
                filter &= newFilter;
            }

            return filter;
        }

        private static FilterDefinition<PersonCategoryCounter> AddNameFilter(PersonCategoryFilter personCategoryFilter, 
            FilterDefinition<PersonCategoryCounter> filter)
        {
            if (!String.IsNullOrEmpty(personCategoryFilter.Name))
            {
                var nameFilter = Builders<PersonCategoryCounter>
                    .Filter.Where(x => x.PersonNameCounters.ContainsKey(personCategoryFilter.Name));
                filter &= nameFilter;
            }

            return filter;
        }
    }
}
