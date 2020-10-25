namespace Census.Shared.Bus.Event
{
    public class PersonUpdatedEvent : IntegrationEvent
    {
        public PersonDTO OldPersonData { get; set; }

        public PersonDTO NewPersonData { get; set; }
    }
}
