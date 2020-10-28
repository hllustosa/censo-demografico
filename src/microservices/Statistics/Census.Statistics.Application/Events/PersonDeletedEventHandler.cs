using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using Census.Statistics.Domain.Entities;
using Census.Statistics.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace Census.Statistics.Application.Events
{
    public class PersonDeletedEventHandler : BaseEventHandler, IIntegrationEventHandler<PersonDeletedEvent>
    {
        IPersonCategoryRepository PersonCategoryRepository { get; set; }

        IPersonPerCityCounterRepository PersonPerCityCounterRepository { get; set; }

        ITransactionManager TransactionManager { get; set; }

        ITransaction Transaction { get; set; }

        public PersonDeletedEventHandler(IPersonCategoryRepository personCategoryRepository,
            IPersonPerCityCounterRepository personPerCityCounterRepository,
            ITransactionManager transactionManager)
        {
            PersonCategoryRepository = personCategoryRepository;
            PersonPerCityCounterRepository = personPerCityCounterRepository;
            TransactionManager = transactionManager;
        }

        public async Task Handle(PersonDeletedEvent @event)
        {
            var person = @event.Person;
            var filter = CreateFilter(person);
            Transaction = TransactionManager.BeginTransaction();

            try
            {
                await HandleCategoryCounters(filter);
                await HandleCityCounters(person, filter);
                TransactionManager.CommitTransaction(Transaction);
            }
            catch (Exception e)
            {
                TransactionManager.RollBackTransaction(Transaction);
                throw e;
            }
        }

        private async Task HandleCategoryCounters(PersonCategoryFilter filter)
        {
            var personCategory = await FindGategory(filter);
            DecrementCategoryCounters(filter, personCategory);
            await PersonCategoryRepository.Save(Transaction, personCategory);
        }

        private async Task HandleCityCounters(PersonDTO person, PersonCategoryFilter filter)
        {
            var city = person.Address.City;
            var personPerCityCategory = await PersonPerCityCounterRepository.GetByCity(city);
            DecrementPerCityCounters(filter, personPerCityCategory);
            await PersonPerCityCounterRepository.Save(Transaction, personPerCityCategory);
        }

        private async Task<PersonCategoryCounter> FindGategory(PersonCategoryFilter filter)
        {
            var categories = await PersonCategoryRepository.GetPersonCategoryCounters(Anonimize(filter));
            var personCategory = categories[0];
            return personCategory;
        }
    }
}
