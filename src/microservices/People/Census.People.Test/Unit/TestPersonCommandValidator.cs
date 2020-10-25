using Census.People.Application.Commands;
using Census.People.Application.Validation;
using Census.People.Domain.Values;
using System.Threading.Tasks;
using Xunit;

namespace Census.People.Test.Unit
{
    public class TestPersonCommandValidator
    {
        [Fact]
        public async Task TestValidator_ValidPerson()
        {
            //Arrange
            var validator = new PersonCommandValidator();
            CreatePersonCommand createCommand = MakeCommand();

            //Act
            var result = await validator.ValidateAsync(createCommand);

            //Assert
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task TestValidator_InvalidEducation()
        {
            //Arrange
            var validator = new PersonCommandValidator();
            CreatePersonCommand createCommand = MakeCommand();
            createCommand.Education = "Invalid Education";

            //Act
            var result = await validator.ValidateAsync(createCommand);

            //Assert
            Assert.Equal("Education", result.Errors[0].PropertyName);
        }

        [Fact]
        public async Task TestValidator_InvalidRace()
        {
            //Arrange
            var validator = new PersonCommandValidator();
            CreatePersonCommand createCommand = MakeCommand();
            createCommand.Race = "Invalid Race";

            //Act
            var result = await validator.ValidateAsync(createCommand);

            //Assert
            Assert.Equal("Race", result.Errors[0].PropertyName);
        }

        [Fact]
        public async Task TestValidator_InvalidSex()
        {
            //Arrange
            var validator = new PersonCommandValidator();
            CreatePersonCommand createCommand = MakeCommand();
            createCommand.Sex = "S";

            //Act
            var result = await validator.ValidateAsync(createCommand);

            //Assert
            Assert.Equal("Sex", result.Errors[0].PropertyName);
        }


        private static CreatePersonCommand MakeCommand()
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
                    ZipCode = "259000000",
                    AddressDesc = "A Street, 100",
                    Burrow = "Neighborhood",
                    City = "City",
                    Complement = "House 1",
                    State = "ST"
                }
            };
        }
    }
}
