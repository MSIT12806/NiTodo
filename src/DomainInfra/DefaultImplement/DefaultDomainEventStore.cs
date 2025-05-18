using System.Collections.Generic;

namespace DomainInfra.DefaultImplement
{
    public class DefaultDomainEventStore<TEvent> : IDomainEventStore<TEvent> where TEvent : IDomainEvent
    {
        private static readonly List<TEvent> _events = new List<TEvent>();
        public void Add(TEvent domainEvent)
        {
            _events.Add(domainEvent);
        }
        public IEnumerable<TEvent> GetUnpublishedEvents()
        {
            return _events;
        }
    }
}
