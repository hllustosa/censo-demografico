using System;
using System.Collections.Generic;
using System.Text;

namespace Census.Shared.Bus.Event
{
    public class PersonCreatedEvent : IntegrationEvent
    {
        public string Message { get; set; }
    }
}
