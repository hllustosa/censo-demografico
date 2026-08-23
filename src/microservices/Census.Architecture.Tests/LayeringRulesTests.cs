using NetArchTest.Rules;
using Xunit;

namespace Census.Architecture.Tests;

public class LayeringRulesTests
{
    [Fact]
    public void People_Domain_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(typeof(Census.People.Domain.Entities.Person).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Census.People.Infra",
                "Census.People.Api",
                "Census.People.Application")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void People_Application_Should_Not_Depend_On_Api_Or_Infra()
    {
        var result = Types.InAssembly(typeof(Census.People.Application.Commands.CreatePersonCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Census.People.Api", "Census.People.Infra")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Statistics_Domain_Should_Not_Depend_On_People_Internals()
    {
        var result = Types.InAssembly(typeof(Census.Statistics.Domain.Entities.PersonCategoryCounter).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Census.People.Domain",
                "Census.People.Application",
                "Census.People.Infra",
                "Census.People.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void FamilyTree_Domain_Should_Not_Depend_On_People_Internals()
    {
        var result = Types.InAssembly(typeof(Census.FamilyTree.Domain.Entities.PersonFamilyTree).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Census.People.Domain",
                "Census.People.Application",
                "Census.People.Infra",
                "Census.People.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
