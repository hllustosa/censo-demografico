namespace Census.Contracts.Events;

public class PersonUpdatedEvent : IntegrationEvent
{
    public PersonDTO OldPersonData { get; set; } = new();

    public PersonDTO NewPersonData { get; set; } = new();
}
