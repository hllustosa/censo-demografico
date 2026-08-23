using Census.People.Application.Commands;
using Census.People.Application.Services;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.People.Domain.Values;
using FluentValidation;
using Moq;
using Xunit;

namespace Census.People.Test.Unit
{
    public class TestUpdatePersonCommand
    {
        private readonly UpdatePersonHandler _updateCommandHandler;
        private readonly Mock<IPersonRepository> _personRepository = new();
        private readonly Mock<IIntegrationEventPublisher> _eventPublisher = new();

        public TestUpdatePersonCommand()
        {
            _updateCommandHandler = new UpdatePersonHandler(_personRepository.Object, _eventPublisher.Object);
        }

        [Fact]
        public async Task TestUpdateValidPerson()
        {
            SetupUpdate();
            SetupGetPersonById(new Person());
            SetupAncestorChecks(false);
            var command = CreateUpdatePersonCommand();

            await _updateCommandHandler.Handle(command, CancellationToken.None);
        }

        [Fact]
        public async Task TestUpdatePersonInvalidFather()
        {
            // Arrange
            SetupUpdate();
            SetupGetPersonById("1", new Person());
            SetupGetPersonById("2", null);
            SetupGetPersonById("3", new Person());
            SetupAncestorChecks(false);
            var command = CreateUpdatePersonCommand();

            // Act & assert
            await Assert.ThrowsAsync<ValidationException>(
                async () => await _updateCommandHandler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task TestUpdatePersonInvalidMother()
        {
            SetupUpdate();
            SetupGetPersonById("1", new Person());
            SetupGetPersonById("2", new Person());
            SetupGetPersonById("3", null);
            SetupAncestorChecks(false);
            var command = CreateUpdatePersonCommand();

            await Assert.ThrowsAsync<ValidationException>(
                async () => await _updateCommandHandler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task TestUpdatePersonCycleDetected()
        {
            SetupUpdate();
            SetupGetPersonById("1", new Person { Id = "1" });
            SetupGetPersonById("2", new Person { Id = "2" });
            SetupGetPersonById("3", new Person { Id = "3" });
            _personRepository
                .Setup(x => x.IsAncestorOf("1", "2"))
                .ReturnsAsync(true);
            _personRepository
                .Setup(x => x.IsAncestorOf("1", "3"))
                .ReturnsAsync(false);

            var command = CreateUpdatePersonCommand();

            var exception = await Assert.ThrowsAsync<ValidationException>(
                async () => await _updateCommandHandler.Handle(command, CancellationToken.None));

            Assert.Contains(exception.Errors, error => error.PropertyName == "FatherId");
        }

        private void SetupGetPersonById(string id, Person? person)
        {
            _personRepository.Setup(x => x.GetPersonById(id)).Returns(Task.FromResult(person));
        }

        private void SetupGetPersonById(Person person)
        {
            _personRepository.Setup(x => x.GetPersonById(It.IsAny<string>())).Returns(Task.FromResult<Person?>(person));
        }

        private void SetupUpdate()
        {
            _personRepository.Setup(x => x.Update(It.IsAny<Person>())).Returns(Task.CompletedTask);
        }

        private void SetupAncestorChecks(bool isAncestor)
        {
            _personRepository
                .Setup(x => x.IsAncestorOf(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(isAncestor);
        }

        private static UpdatePersonCommand CreateUpdatePersonCommand()
        {
            return new UpdatePersonCommand()
            {
                Id = "1",
                Name = "PersonName",
                Education = Education.COLLEGE,
                Race = Race.BLACK,
                Sex = Sex.FEMALE,
                FatherId = "2",
                MotherId = "3",
                Address = new Address()
                {
                    City = "City",
                }
            };
        }
    }
}
