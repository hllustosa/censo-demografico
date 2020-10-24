using Census.People.Domain.Values;
using MediatR;
using System;

namespace Census.People.Application.Commands
{
    public class CreatePersonCommand : BasePersonCommand, IRequest<CreatedPerson> { }

    public class CreatedPerson
    {
        public string Id { get; set; }

        public override bool Equals(object obj)
        {
            return obj is CreatedPerson person &&
                   Id == person.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
