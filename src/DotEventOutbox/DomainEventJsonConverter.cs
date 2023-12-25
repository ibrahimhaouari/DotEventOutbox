using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotEventOutbox.Contracts;

namespace DotEventOutbox;

internal static class DomainEventJsonConverter
{
    public static string Serialize(DomainEvent domainEvent)
    {
        return JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
    }

    public static TEvent? Deserialize<TEvent>(string json) where TEvent : DomainEvent
    {
        var domainEvent = JsonConvert.DeserializeObject<TEvent>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
        });

        return domainEvent;
    }
}
