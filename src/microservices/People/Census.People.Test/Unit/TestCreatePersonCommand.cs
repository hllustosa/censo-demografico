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
    public class TestCreatePersonCommand
    {
        private readonly CreatePersonHandler _createPersonHandler;
        private readonly Mock<IPersonRepository> _personRepository = new();
        private readonly Mock<IIntegrationEventPublisher> _eventPublisher = new();
        private readonly Mock<ITransactionManager> _transactionManager = new();
        private readonly Mock<ITransaction> _transaction = new();

        public TestCreatePersonCommand()
        {
            _transactionManager
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_transaction.Object);
            _transactionManager
                .Setup(x => x.CommitAsync(It.IsAny<ITransaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _transactionManager
                .Setup(x => x.RollbackAsync(It.IsAny<ITransaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _createPersonHandler = new CreatePersonHandler(
                _personRepository.Object,
                _eventPublisher.Object,
                _transactionManager.Object);
        }

        [Fact]
        public async Task TestCreateValidPerson()
        {
            SetupSave();
            SetupGetPersonById(new Person());
            var command = CreatePersonCommand();

            var result = await _createPersonHandler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            _transactionManager.Verify(x => x.CommitAsync(_transaction.Object, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestCreatePersonInvalidFather()
        {
            SetupSave();
            SetupGetPersonById("1", null);
            SetupGetPersonById("2", new Person());
            var command = CreatePersonCommand();

            await Assert.ThrowsAsync<ValidationException>(
                async () => await _createPersonHandler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task TestCreatePersonInvalidMother()
        {
            SetupSave();
            SetupGetPersonById("1", new Person());
            SetupGetPersonById("2", null);
            var command = CreatePersonCommand();

            await Assert.ThrowsAsync<ValidationException>(
                async () => await _createPersonHandler.Handle(command, CancellationToken.None));
        }

        private void SetupGetPersonById(string id, Person? person)
        {
            _personRepository.Setup(x => x.GetPersonById(id)).Returns(Task.FromResult(person));
        }

        private void SetupGetPersonById(Person person)
        {
            _personRepository.Setup(x => x.GetPersonById(It.IsAny<string>())).Returns(Task.FromResult<Person?>(person));
        }

        private void SetupSave()
        {
            _personRepository
                .Setup(x => x.Save(It.IsAny<Person>(), It.IsAny<ITransaction?>()))
                .Returns(Task.CompletedTask);
        }

        private static CreatePersonCommand CreatePersonCommand()
        {
            return new CreatePersonCommand()
            {
                Name = "PersonName",
                Education = Education.COLLEGE,
                Race = Race.BLACK,
                Sex = Sex.FEMALE,
                FatherId = "1",
                MotherId = "2",
                Address = new Address()
                {
                    City = "City",
                }
            };
        }
    }
}
