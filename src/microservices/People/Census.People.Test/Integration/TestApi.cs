using Census.People.Application.Commands;
using Census.People.Domain.Entities;
using Census.People.Domain.Values;
using Census.People.Test.Utils;
using Newtonsoft.Json;
using Xunit;

namespace Census.People.Test.Integration;

[Collection("PeopleIntegration")]
public class TestApi
{
    private readonly TestContext _testContext;

    public TestApi(MongoFixture mongoFixture)
    {
        _testContext = new TestContext(mongoFixture);
    }

    [Fact]
    public async Task TestCreatePerson()
    {
        await DeletePerson();
        await CreatePerson();
        await CheckRetrievedPerson(CreatePersonObject());
    }

    [Fact]
    public async Task TestUpdatePerson()
    {
        await DeletePerson();
        await CreatePerson();
        await UpdatePerson();
        await CheckRetrievedPerson(CreateAfterUpdatePersonObject());
    }

    [Fact]
    public async Task TestDeletePerson()
    {
        await DeletePerson();
        await CreatePerson();
        await DeletePerson();
        await CheckEmptyRetrievedPerson();
    }

    private async Task CreatePerson()
    {
        var createPersonCommand = MakeCreateCommand();
        var result = await _testContext.Post("/api/v1/person", createPersonCommand);
        var resultString = await result.Content.ReadAsStringAsync();
        var createdPerson = JsonConvert.DeserializeObject<CreatedPerson>(resultString);
        Assert.Equal(new CreatedPerson() { Id = "id" }, createdPerson);
    }

    private async Task UpdatePerson()
    {
        var updatePersonCommand = MakeUpdateCommand();
        var result = await _testContext.Put("/api/v1/person/id", updatePersonCommand);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, result.StatusCode);
    }

    private async Task DeletePerson()
    {
        var result = await _testContext.Delete("/api/v1/person/id");
        // Cleanup may run before the person exists.
        Assert.True(
            result.StatusCode is System.Net.HttpStatusCode.NoContent or System.Net.HttpStatusCode.NotFound,
            $"Unexpected delete status: {result.StatusCode}");
    }

    private async Task CheckRetrievedPerson(Person expected)
    {
        var result = await _testContext.Get("/api/v1/person/id");
        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        var resultString = await result.Content.ReadAsStringAsync();
        var person = JsonConvert.DeserializeObject<Person>(resultString);
        Assert.Equal(expected, person);
    }

    private async Task CheckEmptyRetrievedPerson()
    {
        var result = await _testContext.Get("/api/v1/person/id");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
    }

    private Person CreatePersonObject()
    {
        return new Person()
        {
            Id = "id",
            Name = "PersonName",
            Education = Education.COLLEGE,
            Race = Race.BLACK,
            Sex = Sex.FEMALE,
            Address = new Address()
            {
                AddressDesc = "Desc",
                Complement = "",
                ZipCode = "25000-000",
                City = "City",
                Burrow = "Burrow",
                State = "ST"
            }
        };
    }

    private CreatePersonCommand MakeCreateCommand()
    {
        return new CreatePersonCommand()
        {
            Name = "PersonName",
            Education = Education.COLLEGE,
            Race = Race.BLACK,
            Sex = Sex.FEMALE,
            Address = new Address()
            {
                AddressDesc = "Desc",
                Complement = "",
                ZipCode = "25000-000",
                City = "City",
                Burrow = "Burrow",
                State = "ST"
            }
        };
    }

    private CreatePersonCommand MakeUpdateCommand()
    {
        return new CreatePersonCommand()
        {
            Id = "id",
            Name = "PersonName2",
            Education = Education.ELEMENTARY,
            Race = Race.ASIAN,
            Sex = Sex.MALE,
            Address = new Address()
            {
                AddressDesc = "Desc2",
                Complement = "cmp",
                ZipCode = "25000-001",
                City = "City2",
                Burrow = "Burrow2",
                State = "SA"
            }
        };
    }

    private Person CreateAfterUpdatePersonObject()
    {
        return new Person()
        {
            Id = "id",
            Name = "PersonName2",
            Education = Education.ELEMENTARY,
            Race = Race.ASIAN,
            Sex = Sex.MALE,
            Address = new Address()
            {
                AddressDesc = "Desc2",
                Complement = "cmp",
                ZipCode = "25000-001",
                City = "City2",
                Burrow = "Burrow2",
                State = "SA"
            }
        };
    }
}
