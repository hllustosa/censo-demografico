using Census.People.Application.Commands;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.People.Domain.Values;
using FluentValidation;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Census.People.Test.Unit
{
    public class TestUpdatePersonCommand
    {
        private readonly UpdatePersonHandler UpdateCommandHandler;

        private readonly Mock<IPersonRepository> PersonRepository = new Mock<IPersonRepository>();

        public TestUpdatePersonCommand()
        {
            UpdateCommandHandler = new UpdatePersonHandler(PersonRepository.Object);
        }

        [Fact]
        public async void TestUpdateValidPerson()
        {
            // Arrange
            SetupUpdate();
            SetupGetPersonById(new Person());
            var command = CreateUpdatePersonCommand();

            // Act
            var result = await UpdateCommandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<MediatR.Unit>(result);
        }

        [Fact]
        public async void TestUpdatePersonInvalidFather()
        {
            // Arrange
            SetupUpdate();
            SetupGetPersonById("1", new Person());
            SetupGetPersonById("2", null);
            SetupGetPersonById("3", new Person());
            var command = CreateUpdatePersonCommand();

            // Act & assert
            await Assert.ThrowsAsync<ValidationException>(
                async () => await UpdateCommandHandler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async void TestUpdatePersonInvalidMother()
        {
            // Arrange
            SetupUpdate();
            SetupGetPersonById("1", new Person());
            SetupGetPersonById("2", new Person());
            SetupGetPersonById("3", null);
            var command = CreateUpdatePersonCommand();

            // Act & assert
            await Assert.ThrowsAsync<ValidationException>(
                async () => await UpdateCommandHandler.Handle(command, CancellationToken.None));
        }

        private void SetupGetPersonById(string id, Person person)
        {
            PersonRepository.Setup(x => x.GetPersonById(id)).Returns(Task.FromResult<Person>(person));
        }

        private void SetupGetPersonById(Person person)
        {
            PersonRepository.Setup(x => x.GetPersonById(It.IsAny<string>())).Returns(Task.FromResult<Person>(person));
        }

        private void SetupUpdate()
        {
            PersonRepository.Setup(x => x.Update(It.IsAny<Person>()));
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
            };
        }
    }
}
