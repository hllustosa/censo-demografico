using System.Threading.Tasks;
using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;

namespace Census.FamilyTree.Application.Events
{
    public class PersonUpdatedEventHandler : IIntegrationEventHandler<PersonUpdatedEvent>
    {
        IPersonFamilyTreeRepository PersonFamilyTreeRepository { get; set; }

        public PersonUpdatedEventHandler(IPersonFamilyTreeRepository personFamilyTreeRepository)
        {
            PersonFamilyTreeRepository = personFamilyTreeRepository;
        }

        public async Task Handle(PersonUpdatedEvent @event)
        {
            var oldNode = ToNode(@event.OldPersonData);
            var newNode = ToNode(@event.NewPersonData);

            if (HasChangedParents(@event))
            {
                await PersonFamilyTreeRepository.UpdateNode(oldNode, newNode);
                return;
            }

            await PersonFamilyTreeRepository.AddNode(newNode);
        }

        private static bool HasChangedParents(PersonUpdatedEvent @event)
        {
            return @event.OldPersonData.FatherId != @event.NewPersonData.FatherId
                || @event.OldPersonData.MotherId != @event.NewPersonData.MotherId
                || @event.OldPersonData.Name != @event.NewPersonData.Name;
        }

        private static PersonFamilyTreeNode ToNode(PersonDTO person)
        {
            return new PersonFamilyTreeNode
            {
                Id = person.Id,
                Name = person.Name,
                FatherId = person.FatherId,
                MotherId = person.MotherId,
            };
        }
    }
}
