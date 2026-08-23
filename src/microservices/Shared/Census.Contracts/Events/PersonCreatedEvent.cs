namespace Census.Contracts.Events;

public class PersonCreatedEvent : IntegrationEvent
{
    public PersonDTO Person { get; set; } = new();
}
