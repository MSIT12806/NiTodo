namespace DomainInfra
{
    public interface IDomainEventHandler
    {
        void Handle(IDomainEvent domainEvent);
        bool IsThisEvent(IDomainEvent domainEvent);
    }
    public interface IDomainEventHandler<TEvent> : IDomainEventHandler where TEvent : IDomainEvent
    {
        void Handle(TEvent domainEvent);
    }

}
