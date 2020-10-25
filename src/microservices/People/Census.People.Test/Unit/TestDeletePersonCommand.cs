using Census.People.Application.Commands;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.Shared.Bus.Interfaces;
using FluentValidation;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Census.People.Test.Unit
{
    public class TestDeletePersonCommand
    {
        private readonly DeletePersonHandler DeletePersonHandler;

        private readonly Mock<IPersonRepository> PersonRepository = new Mock<IPersonRepository>();

        private readonly Mock<IEventBus> EventBus = new Mock<IEventBus>();

        public TestDeletePersonCommand()
        {
            DeletePersonHandler = new DeletePersonHandler(PersonRepository.Object, EventBus.Object);
        }

        [Fact]
        public async Task TestDeleteExistingPerson()
        {
            // Arrange
            SetupGetPersonById(new Person());
            var command = new DeletePersonCommand() { Id = "1"};

            // Act
            var result = await DeletePersonHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<MediatR.Unit>(result);
        }

        [Fact]
        public async Task TestDeleteNonExistingPerson()
        {
            // Arrange
            var command = new DeletePersonCommand() { Id = "1" };

            // Act & assert
            await Assert.ThrowsAsync<ValidationException>(
                async () => await DeletePersonHandler.Handle(command, CancellationToken.None));
        }

        private void SetupGetPersonById(Person person)
        {
            PersonRepository.Setup(x => x.GetPersonById(It.IsAny<string>())).Returns(Task.FromResult<Person>(person));
        }
    }
}
