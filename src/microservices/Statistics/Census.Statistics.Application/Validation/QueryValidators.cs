using FluentValidation;
using Census.Statistics.Application.Queries;

namespace Census.Statistics.Application.Validation
{
    public class PersonCategoryQueryValidator : AbstractValidator<PersonCategoryQuery>
    {
        public PersonCategoryQueryValidator()
        {
            RuleFor(query => query.PersonCategoryFilter).NotNull();
        }
    }

    public class PersonPerCityCounterCityFilterQueryValidator : AbstractValidator<PersonPerCityCounterCityFilterQuery>
    {
        public PersonPerCityCounterCityFilterQueryValidator()
        {
            RuleFor(query => query.CityNameFilter).NotEmpty();
        }
    }
}
