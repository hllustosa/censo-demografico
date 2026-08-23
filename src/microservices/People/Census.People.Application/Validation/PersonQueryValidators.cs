using Census.People.Application.Commands;
using FluentValidation;

namespace Census.People.Application.Validation;

public class PersonByIdQueryValidator : AbstractValidator<Queries.PersonByIdQuery>
{
    public PersonByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id é obrigatório.");
    }
}

public class DeletePersonCommandValidator : AbstractValidator<DeletePersonCommand>
{
    public DeletePersonCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id é obrigatório.");
    }
}
