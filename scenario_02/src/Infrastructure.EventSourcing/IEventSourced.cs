using System.Collections.Generic;

namespace Infrastructure.EventSourcing
{
    public interface IEventSourced<out TId>
    {
        TId Id { get; }
        int Version { get; }
        IEnumerable<IVersionedEvent<TId>> PendingEvents { get; }
    }
}