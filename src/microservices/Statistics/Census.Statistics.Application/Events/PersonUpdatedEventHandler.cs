using System;
using System.Threading.Tasks;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using Census.Statistics.Domain.Entities;
using Census.Statistics.Domain.Interfaces;

namespace Census.Statistics.Application.Events
{
    public class PersonUpdatedEventHandler : BaseEventHandler, IIntegrationEventHandler<PersonUpdatedEvent>
    {
        IPersonCategoryRepository PersonCategoryRepository { get; set; }

        IPersonPerCityCounterRepository PersonPerCityCounterRepository { get; set; }

        ITransactionManager TransactionManager { get; set; }

        ITransaction Transaction { get; set; }

        public PersonUpdatedEventHandler(IPersonCategoryRepository personCategoryRepository,
            IPersonPerCityCounterRepository personPerCityCounterRepository,
            ITransactionManager transactionManager)
        {
            PersonCategoryRepository = personCategoryRepository;
            PersonPerCityCounterRepository = personPerCityCounterRepository;
            TransactionManager = transactionManager;
        }

        public async Task Handle(PersonUpdatedEvent @event)
        {
            var oldPerson = @event.OldPersonData;
            var newPerson = @event.NewPersonData;
            var oldFilter = CreateFilter(oldPerson);
            var newFilter = CreateFilter(newPerson);
            Transaction = TransactionManager.BeginTransaction();

            try
            {
                await HandleCategoryCounters(oldFilter, newFilter);
                await HandleCityCounters(oldPerson.Address.City, newPerson.Address.City,
                    oldFilter, newFilter);
                TransactionManager.CommitTransaction(Transaction);
            }
            catch(Exception e)
            {
                TransactionManager.RollBackTransaction(Transaction);
                throw e;
            }
        }

        private async Task HandleCategoryCounters(PersonCategoryFilter oldFilter,
            PersonCategoryFilter newFilter)
        {
            var oldPersonCategory = await FindGategory(oldFilter);
            DecrementCategoryCounters(oldFilter, oldPersonCategory);
            await PersonCategoryRepository.Save(Transaction, oldPersonCategory);

            var newPersonCategory = await FindGategory(newFilter);
            IncrementCategoryCounters(newFilter, newPersonCategory);
            await PersonCategoryRepository.Save(Transaction, newPersonCategory);
        }

        private async Task HandleCityCounters(string oldCity, string newCity, 
            PersonCategoryFilter oldFilter, PersonCategoryFilter newFilter)
        {
            if(oldCity != newCity)
            {
                var oldPersonPerCityCategory = await PersonPerCityCounterRepository.GetByCity(oldCity);
                DecrementPerCityCounters(oldFilter, oldPersonPerCityCategory);
                await PersonPerCityCounterRepository.Save(Transaction, oldPersonPerCityCategory);

                var newPersonPerCityCategory = await PersonPerCityCounterRepository.GetByCity(newCity);
                IncrementPerCityCounters(newFilter, newPersonPerCityCategory);
                await PersonPerCityCounterRepository.Save(Transaction, newPersonPerCityCategory);
            }
        }

        private async Task<PersonCategoryCounter> FindGategory(PersonCategoryFilter filter)
        {
            var categories = await PersonCategoryRepository.GetPersonCategoryCounters(filter);
            var personCategory = categories[0];
            return personCategory;
        }
    }
}
