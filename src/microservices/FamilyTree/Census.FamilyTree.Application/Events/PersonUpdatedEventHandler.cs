using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using System.Threading.Tasks;

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
            if(HasChangedParents(@event))
            {
                var oldNode = new PersonFamilyTreeNode()
                {
                    Id = @event.OldPersonData.Id,
                    Name = @event.OldPersonData.Name,
                    FatherId = @event.OldPersonData.FatherId,
                    MotherId = @event.OldPersonData.MotherId
                };

                var newNode = new PersonFamilyTreeNode()
                {
                    Id = @event.NewPersonData.Id,
                    Name = @event.NewPersonData.Name,
                    FatherId = @event.NewPersonData.FatherId,
                    MotherId = @event.NewPersonData.MotherId
                };

                await PersonFamilyTreeRepository.UpdateNode(oldNode, newNode);
            }
        }

        private bool HasChangedParents(PersonUpdatedEvent @event)
        {
            return @event.OldPersonData.FatherId != @event.NewPersonData.FatherId
                || @event.OldPersonData.MotherId != @event.NewPersonData.MotherId;
        }
    }
}
