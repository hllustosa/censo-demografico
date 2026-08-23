using Census.People.Application.Commands;
using Census.People.Application.Services;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.Shared.Web.Exceptions;
using Moq;
using Xunit;

namespace Census.People.Test.Unit
{
    public class TestDeletePersonCommand
    {
        private readonly DeletePersonHandler _deletePersonHandler;
        private readonly Mock<IPersonRepository> _personRepository = new();
        private readonly Mock<IIntegrationEventPublisher> _eventPublisher = new();
        private readonly Mock<ITransactionManager> _transactionManager = new();
        private readonly Mock<ITransaction> _transaction = new();

        public TestDeletePersonCommand()
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

            _deletePersonHandler = new DeletePersonHandler(
                _personRepository.Object,
                _eventPublisher.Object,
                _transactionManager.Object);
        }

        [Fact]
        public async Task TestDeleteExistingPerson()
        {
            SetupGetPersonById(new Person());
            _personRepository
                .Setup(x => x.Delete(It.IsAny<string>(), It.IsAny<ITransaction?>()))
                .Returns(Task.CompletedTask);
            var command = new DeletePersonCommand { Id = "1" };

            await _deletePersonHandler.Handle(command, CancellationToken.None);
            _transactionManager.Verify(x => x.CommitAsync(_transaction.Object, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestDeleteNonExistingPerson()
        {
            var command = new DeletePersonCommand { Id = "1" };

            await Assert.ThrowsAsync<NotFoundException>(
                async () => await _deletePersonHandler.Handle(command, CancellationToken.None));
        }

        private void SetupGetPersonById(Person person)
        {
            _personRepository.Setup(x => x.GetPersonById(It.IsAny<string>())).Returns(Task.FromResult<Person?>(person));
        }
    }
}
