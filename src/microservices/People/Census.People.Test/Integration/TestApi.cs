using System;
using System.Collections.Generic;
using Census.People.Application.Commands;
using Census.People.Domain.Entities;
using Census.People.Domain.Values;
using Census.People.Test.Utils;
using Newtonsoft.Json;
using Xunit;

public class TestApi
{
    TestContext TestContext { get; set; }

    public TestApi()
    {
        TestContext = new TestContext();
    }

    [Fact]
    public async void TestCreatePerson()
    {
        var createPersonCommand = MakeCreateCommand();

        var result = await TestContext.Post("/api/person", createPersonCommand);
        var resultString = await result.Content.ReadAsStringAsync();
        var createdPerson = JsonConvert.DeserializeObject<CreatedPerson>(resultString);
        Assert.Equal(new CreatedPerson() { Id = "id" }, createdPerson);

        result = await TestContext.Get("/api/person/id");
        resultString = await result.Content.ReadAsStringAsync();
        var person = JsonConvert.DeserializeObject<Person>(resultString);
        Assert.Equal(CreatePerson(), person);

    }

    private Person CreatePerson()
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
}