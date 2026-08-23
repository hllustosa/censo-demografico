using Census.People.Application.Commands;
using Census.People.Application.Services;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using FluentValidation;
using Moq;
using Xunit;

namespace Census.People.Test.Unit
{
    public class TestDeletePersonCommand
    {
        private readonly DeletePersonHandler _deletePersonHandler;
        private readonly Mock<IPersonRepository> _personRepository = new();
        private readonly Mock<IIntegrationEventPublisher> _eventPublisher = new();

        public TestDeletePersonCommand()
        {
            _deletePersonHandler = new DeletePersonHandler(_personRepository.Object, _eventPublisher.Object);
        }

        [Fact]
        public async Task TestDeleteExistingPerson()
        {
            SetupGetPersonById(new Person());
            var command = new DeletePersonCommand { Id = "1" };

            await _deletePersonHandler.Handle(command, CancellationToken.None);
        }

        [Fact]
        public async Task TestDeleteNonExistingPerson()
        {
            var command = new DeletePersonCommand { Id = "1" };

            await Assert.ThrowsAsync<ValidationException>(
                async () => await _deletePersonHandler.Handle(command, CancellationToken.None));
        }

        private void SetupGetPersonById(Person person)
        {
            _personRepository.Setup(x => x.GetPersonById(It.IsAny<string>())).Returns(Task.FromResult<Person?>(person));
        }
    }
}
