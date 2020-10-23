using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using System;
using System.Threading.Tasks;

namespace Census.Statistics.Application.Events
{
    public class PersonCreatedEventHandler : IIntegrationEventHandler<PersonCreatedEvent>
    {
        public Task Handle(PersonCreatedEvent @event)
        {
            Console.WriteLine(@event.Message);
            return Task.CompletedTask;
        }
    }
}
