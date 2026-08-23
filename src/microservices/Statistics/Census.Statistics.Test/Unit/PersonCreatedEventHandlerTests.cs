using Census.Shared.Bus.Event;
using Census.Statistics.Application;
using Census.Statistics.Application.Events;
using Census.Statistics.Domain.Entities;
using Census.Statistics.Domain.Interfaces;
using Moq;
using Xunit;

namespace Census.Statistics.Test.Unit;

public class PersonCreatedEventHandlerTests
{
    [Fact]
    public async Task Handle_IncrementsCategoryCounter()
    {
        var categoryRepository = new Mock<IPersonCategoryRepository>();
        var cityRepository = new Mock<IPersonPerCityCounterRepository>();
        var transactionManager = new Mock<ITransactionManager>();
        var sender = new Mock<INotificationSender>();
        var transaction = new Mock<ITransaction>();

        categoryRepository
            .Setup(repository => repository.GetPersonCategoryCounters(It.IsAny<PersonCategoryFilter>()))
            .ReturnsAsync(new List<PersonCategoryCounter>
            {
                new() { Count = 0, Sex = "F" }
            });

        cityRepository
            .Setup(repository => repository.GetByCity(It.IsAny<string>()))
            .ReturnsAsync(new PersonPerCityCounter());

        transactionManager.Setup(manager => manager.BeginTransaction()).Returns(transaction.Object);

        var handler = new PersonCreatedEventHandler(
            categoryRepository.Object,
            cityRepository.Object,
            transactionManager.Object,
            sender.Object);

        await handler.Handle(new PersonCreatedEvent
        {
            Person = new PersonDTO
            {
                Name = "Jane",
                Sex = "F",
                Race = "W",
                Education = "C",
                Address = new AddressDTO { City = "Rio" }
            }
        });

        categoryRepository.Verify(repository => repository.Save(transaction.Object, It.IsAny<PersonCategoryCounter>()), Times.Once);
        sender.Verify(s => s.NotifyAll(), Times.Once);
    }
}
