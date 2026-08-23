using Census.FamilyTree.Application.Queries;
using FluentValidation;

namespace Census.FamilyTree.Application.Validation
{
    public class FamilyTreeQueryValidator : AbstractValidator<FamilyTreeQuery>
    {
        public FamilyTreeQueryValidator()
        {
            RuleFor(query => query.PersonId).NotEmpty();
            RuleFor(query => query.Level).GreaterThan((uint)0).LessThanOrEqualTo((uint)10);
        }
    }
}
