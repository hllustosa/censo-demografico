using Census.Contracts.Events;

namespace Census.Shared.Bus
{
    /// <summary>
    /// Compatibility alias. Prefer <see cref="Census.Contracts.Events.IntegrationEvent"/>.
    /// </summary>
    public class IntegrationEvent : Census.Contracts.Events.IntegrationEvent
    {
        public IntegrationEvent()
        {
        }

        public IntegrationEvent(Guid id, DateTime createDate, string? correlationId = null)
            : base(id, createDate, correlationId)
        {
        }
    }
}

namespace Census.Shared.Bus.Event
{
    /// <summary>
    /// Compatibility wrappers. New code should prefer Census.Contracts.Events.*.
    /// </summary>
    public class PersonCreatedEvent : Census.Shared.Bus.IntegrationEvent
    {
        public PersonDTO Person { get; set; } = new();
    }

    public class PersonUpdatedEvent : Census.Shared.Bus.IntegrationEvent
    {
        public PersonDTO OldPersonData { get; set; } = new();

        public PersonDTO NewPersonData { get; set; } = new();
    }

    public class PersonDeletedEvent : Census.Shared.Bus.IntegrationEvent
    {
        public PersonDTO Person { get; set; } = new();
    }

    public class PersonDTO : Census.Contracts.Events.PersonDTO
    {
    }

    public class AddressDTO : Census.Contracts.Events.AddressDTO
    {
    }
}
