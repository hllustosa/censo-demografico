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
    public class TestCreatePersonCommand
    {
        private readonly CreatePersonHandler CreatePersonHandler;

        private readonly Mock<IPersonRepository> PersonRepository = new Mock<IPersonRepository>();

        public TestCreatePersonCommand()
        {
            CreatePersonHandler = new CreatePersonHandler(PersonRepository.Object);
        }

        [Fact]
        public async void TestCreateValidPerson()
        {
            // Arrange
            SetupSave();
            SetupGetPersonById(new Person());
            var command = CreatePersonCommand();

            // Act
            var result = await CreatePersonHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async void TestCreatePersonInvalidFather()
        {
            // Arrange
            SetupSave();
            SetupGetPersonById("1", null);
            SetupGetPersonById("2", new Person());            
            var command = CreatePersonCommand();

            // Act & assert
            await Assert.ThrowsAsync<ValidationException>(
                async () => await CreatePersonHandler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async void TestCreatePersonInvalidMother()
        {
            // Arrange
            SetupSave();
            SetupGetPersonById("1", new Person());
            SetupGetPersonById("2", null);
            var command = CreatePersonCommand();

            // Act & assert
            await Assert.ThrowsAsync<ValidationException>(
                async () => await CreatePersonHandler.Handle(command, CancellationToken.None));
        }

        private void SetupGetPersonById(string id, Person person)
        {
            PersonRepository.Setup(x => x.GetPersonById(id)).Returns(Task.FromResult<Person>(person));
        }

        private void SetupGetPersonById(Person person)
        {
            PersonRepository.Setup(x => x.GetPersonById(It.IsAny<string>())).Returns(Task.FromResult<Person>(person));
        }

        private void SetupSave()
        {
            PersonRepository.Setup(x => x.Save(It.IsAny<Person>()));
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
            };
        }
    }
}
