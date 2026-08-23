namespace Census.Contracts.Events;

public class PersonDeletedEvent : IntegrationEvent
{
    public PersonDTO Person { get; set; } = new();
}
