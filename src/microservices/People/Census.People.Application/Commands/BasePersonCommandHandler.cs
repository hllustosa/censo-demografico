using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.Shared.Bus.Event;
using Census.Shared.Web.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace Census.People.Application.Commands
{
    public class BasePersonCommandHandler
    {
        readonly IPersonRepository PersonRepository;

        public BasePersonCommandHandler(IPersonRepository personRepository)
        {
            PersonRepository = personRepository;
        }

        public async Task Validate(Person person)
        {
            if (HasSameParent(person))
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new("FatherId", "Pai e mãe não podem ser a mesma pessoa."),
                    new("MotherId", "Pai e mãe não podem ser a mesma pessoa."),
                });
            }

            if (HasDefinedFather(person))
                await CheckIfExists(person.FatherId!, "FatherId");

            if (HasDefinedMother(person))
                await CheckIfExists(person.MotherId!, "MotherId");

            if (!string.IsNullOrEmpty(person.Id))
            {
                await ValidateNoCycle(person, person.FatherId, "FatherId");
                await ValidateNoCycle(person, person.MotherId, "MotherId");
            }
        }

        private async Task ValidateNoCycle(Person person, string? proposedParentId, string field)
        {
            if (string.IsNullOrEmpty(proposedParentId))
            {
                return;
            }

            if (await PersonRepository.IsAncestorOf(person.Id, proposedParentId))
            {
                throw new ValidationException(
                    new List<ValidationFailure>
                    {
                        new(field, "Não é possível definir um descendente como pai ou mãe."),
                    });
            }
        }

        private static bool HasSameParent(Person person)
        {
            return HasDefinedFather(person)
                && HasDefinedMother(person)
                && person.FatherId == person.MotherId;
        }

        public async Task CheckIfExists(string id, string field)
        {
            var person = await PersonRepository.GetPersonById(id);
            if (person == null)
            {
                if (field == "Id")
                {
                    throw new NotFoundException("Pessoa não encontrada.");
                }

                throw new ValidationException(
                    new List<ValidationFailure> { new(field, "Pessoa referenciada não encontrada.") });
            }
        }

        public Person RequestToEntity(BasePersonCommand request)
        {
            return new Person()
            {
                Id = request.Id,
                Name = request.Name,
                Education = request.Education,
                Race = request.Race,
                Sex = request.Sex,
                Address = request.Address,
                FatherId = request.FatherId,
                MotherId = request.MotherId
            };
        }

        private static bool HasDefinedMother(Person person)
        {
            return !String.IsNullOrEmpty(person.MotherId);
        }

        private static bool HasDefinedFather(Person person)
        {
            return !String.IsNullOrEmpty(person.FatherId);
        }

        protected static PersonDTO ToDTO(Person person)
        {
            return new PersonDTO()
            {
                Id = person.Id,
                Address = new AddressDTO()
                {
                    AddressDesc = person.Address?.AddressDesc,
                    Burrow = person.Address?.Burrow,
                    City = person.Address?.City,
                    Complement = person.Address?.Complement,
                    State = person.Address?.State,
                    ZipCode = person.Address?.ZipCode,
                },
                Education = person.Education,
                FatherId = person.FatherId,
                MotherId = person.MotherId,
                Name = person.Name,
                Race = person.Race,
                Sex = person.Sex
            };
        }
    }
}
