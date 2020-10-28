using Census.People.Application.Commands;
using Census.People.Domain.Values;
using FluentValidation;
using System.Linq;

namespace Census.People.Application.Validation
{
    public class PersonCommandValidator : AbstractValidator<BasePersonCommand>
    {
        public PersonCommandValidator()
        {
            RuleFor(x => x.Name).MaximumLength(100).NotNull().NotEmpty();

            RuleFor(x => x.Race).MaximumLength(100).NotNull().NotEmpty()
                .Must(race => Race.Values().Contains(race));

            RuleFor(x => x.Sex).NotNull().NotEmpty()
                .Must(sex => Sex.Values().Contains(sex));

            RuleFor(x => x.Education).MaximumLength(100).NotNull().NotEmpty()
                .Must(education => Education.Values().Contains(education));

            RuleFor(x => x.Address).NotNull();
            RuleFor(x => x.Address.AddressDesc).MaximumLength(300).NotNull();
            RuleFor(x => x.Address.Complement).MaximumLength(300).NotNull();
            RuleFor(x => x.Address.Burrow).MaximumLength(100).NotNull();
            RuleFor(x => x.Address.City).MaximumLength(100).NotNull();
            RuleFor(x => x.Address.State).MaximumLength(2).NotNull();

            //Can't be your own father or mother
            RuleFor(x => x.FatherId).Must((x, fatherId) => fatherId != x.Id);
            RuleFor(x => x.FatherId).Must((x, motherId) => motherId != x.Id);
        }
    }
}
