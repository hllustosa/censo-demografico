using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using System.Threading.Tasks;

namespace Census.FamilyTree.Application.Events
{
    public class PersonCreatedEventHandler : IIntegrationEventHandler<PersonCreatedEvent>
    {
        IPersonFamilyTreeRepository PersonFamilyTreeRepository { get; set; }

        public PersonCreatedEventHandler(IPersonFamilyTreeRepository personFamilyTreeRepository)
        {
            PersonFamilyTreeRepository = personFamilyTreeRepository;
        }

        public async Task Handle(PersonCreatedEvent @event)
        {
            var personFamilyTreeNode = new PersonFamilyTreeNode()
            {
                Id = @event.Person.Id,
                Name = @event.Person.Name,
                FatherId = @event.Person.FatherId,
                MotherId = @event.Person.MotherId
            };

            await PersonFamilyTreeRepository.AddNode(personFamilyTreeNode);
        }
    }
}
