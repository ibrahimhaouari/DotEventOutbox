using DotEventOutbox.Contracts;
using Newtonsoft.Json.Linq;

namespace DotEventOutbox.Tests;

public class DomainEventJsonConverterTests
{
    // Mock domain event for testing
    private record MockDomainEvent(Guid EmitterId) : DomainEvent;

    [Fact]
    public void Serialize_SerializesDomainEvent()
    {
        // Arrange
        var domainEvent = new MockDomainEvent(Guid.NewGuid());

        // Act
        var json = DomainEventJsonConverter.Serialize(domainEvent);

        // Assert
        Assert.NotNull(json);

        // Additional checks to ensure the JSON structure is correct
        var jObject = JObject.Parse(json);
        Assert.Equal(domainEvent.Id.ToString(), jObject["Id"]!.ToString());
        Assert.Equal(domainEvent.OccurredOnUtc, jObject["OccurredOnUtc"]!.ToObject<DateTime>());
        Assert.Equal(domainEvent.EmitterId.ToString(), jObject["EmitterId"]!.ToString());
    }

    [Fact]
    public void Deserialize_DeserializesDomainEvent()
    {
        // Arrange
        var domainEvent = new MockDomainEvent(Guid.NewGuid());
        var json = DomainEventJsonConverter.Serialize(domainEvent);

        // Act
        var deserializedDomainEvent = DomainEventJsonConverter.Deserialize<MockDomainEvent>(json);

        // Assert
        Assert.NotNull(deserializedDomainEvent);
        Assert.Equal(domainEvent.Id, deserializedDomainEvent!.Id);
        Assert.Equal(domainEvent.OccurredOnUtc, deserializedDomainEvent.OccurredOnUtc);
        Assert.Equal(domainEvent.EmitterId, deserializedDomainEvent.EmitterId);
    }
}