namespace Census.Shared.Bus.Event
{
    public class PersonCreatedEvent : IntegrationEvent
    {
        public PersonDTO Person { get; set; }
    }
}
