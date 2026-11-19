namespace Crm.Platform.Messaging;

/// <summary>Event types with at least one subscriber, by full name. Filled at startup by AddEventSubscriber.</summary>
internal sealed class EventTypes
{
    private readonly Dictionary<string, Type> byName = new(StringComparer.Ordinal);

    public void Add(Type eventType) => byName[eventType.FullName!] = eventType;

    public Type? Find(string fullName) => byName.GetValueOrDefault(fullName);
}
