namespace DomainInfra
{
    /// <summary>
    /// 領域事件發布器，負責將領域事件發布到系統外部，如消息隊列、事件總線等
    /// </summary>
    public interface IDomainEventPublisher
    {
        void Publish(IDomainEvent domainEvent);
    }

}
