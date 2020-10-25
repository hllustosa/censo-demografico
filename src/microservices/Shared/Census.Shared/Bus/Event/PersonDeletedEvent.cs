namespace Census.Shared.Bus.Event
{
    public class PersonDeletedEvent : IntegrationEvent
    {
        public PersonDTO Person { get; set; }
    }
}
