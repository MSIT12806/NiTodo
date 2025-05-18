using System.Collections.Generic;

namespace DomainInfra
{
    public interface IDomainEventStore<TEvent> where TEvent : IDomainEvent
    {
        void Add(TEvent domainEvent);
        IEnumerable<TEvent> GetUnpublishedEvents();
    }
}
