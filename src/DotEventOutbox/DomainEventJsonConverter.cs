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

    public static DomainEvent? Deserialize(string json)
    {
        var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
        });

        return domainEvent;
    }
}
