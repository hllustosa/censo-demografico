using Census.People.Application.Queries;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.People.Domain.Values;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Census.People.Test.Unit
{
    public class TestQueryPersonById
    {
        private readonly PersonByIdQueryHandler PersonByIdQueryHandler;

        private readonly Mock<IPersonRepository> PersonRepository = new Mock<IPersonRepository>();

        public TestQueryPersonById()
        {
            PersonByIdQueryHandler = new PersonByIdQueryHandler(PersonRepository.Object);
        }

        [Fact]
        public async Task TestQueryExistingPerson()
        {
            SetupGetPersonById("1", GetPerson());
            var returnedPerson = await PersonByIdQueryHandler.Handle(new PersonByIdQuery() { Id = "1"}, CancellationToken.None);
            Assert.Equal(GetPerson(), returnedPerson);
        }

        [Fact]
        public async Task TestQueryNonExistingPerson()
        {
            var returnedPerson = await PersonByIdQueryHandler.Handle(new PersonByIdQuery() { Id = "1" }, CancellationToken.None);
            Assert.Null(returnedPerson);
        }

        private void SetupGetPersonById(string id, Person person)
        {
            PersonRepository.Setup(x => x.GetPersonById(id)).Returns(Task.FromResult<Person>(person));
        }

        private Person GetPerson()
        {
            return new Person()
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
