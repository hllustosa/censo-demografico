using Census.People.Application.Queries;
using FluentValidation;

namespace Census.People.Application.Validation
{
    public class PeopleQueryValidator : AbstractValidator<PeopleQuery>
    {
        public PeopleQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        }
    }
}
