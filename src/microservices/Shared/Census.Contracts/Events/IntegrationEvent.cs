using Newtonsoft.Json;

namespace Census.Contracts.Events;

public class IntegrationEvent
{
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime createDate, string? correlationId = null)
    {
        Id = id;
        CreationDate = createDate;
        CorrelationId = correlationId;
    }

    [JsonProperty]
    public Guid Id { get; private set; }

    [JsonProperty]
    public DateTime CreationDate { get; private set; }

    [JsonProperty]
    public string? CorrelationId { get; set; }
}
