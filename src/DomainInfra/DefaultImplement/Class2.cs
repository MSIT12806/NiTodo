namespace DomainInfra.DefaultImplement
{
    /*
     * 下列這段程式碼是 chat GPT 模擬實作各種不同的事件發布方式，請參考就好
     */

    //public class SynchronousDomainEventPublisherWorker : IDomainEventPublisherWorker
    //{
    //    private readonly IDomainEventStore _eventStore;
    //    private readonly IDomainEventPublisher _eventPublisher;

    //    public SynchronousDomainEventPublisherWorker(IDomainEventStore eventStore, IDomainEventPublisher eventPublisher)
    //    {
    //        this._eventStore = eventStore;
    //        this._eventPublisher = eventPublisher;
    //    }

    //    public void Start()
    //    {
    //        // 立即發布所有未發送的事件
    //        var events = this._eventStore.GetUnpublishedEvents();
    //        foreach (var domainEvent in events)
    //        {
    //            this._eventPublisher.Publish(domainEvent);
    //        }
    //    }

    //    public void Stop()
    //    {
    //        // 同步模式無需停止邏輯
    //    }
    //}

    //public class BackgroundTaskDomainEventPublisherWorker : IDomainEventPublisherWorker
    //{
    //    private readonly IDomainEventStore _eventStore;
    //    private readonly IDomainEventPublisher _eventPublisher;
    //    private CancellationTokenSource _cancellationTokenSource;

    //    public BackgroundTaskDomainEventPublisherWorker(IDomainEventStore eventStore, IDomainEventPublisher eventPublisher)
    //    {
    //        this._eventStore = eventStore;
    //        this._eventPublisher = eventPublisher;
    //        this._cancellationTokenSource = new CancellationTokenSource();
    //    }

    //    public void Start()
    //    {
    //        Task.Run(() => this.ExecuteAsync(this._cancellationTokenSource.Token));
    //    }

    //    public void Stop()
    //    {
    //        this._cancellationTokenSource.Cancel();
    //    }

    //    private async Task ExecuteAsync(CancellationToken cancellationToken)
    //    {
    //        while (!cancellationToken.IsCancellationRequested)
    //        {
    //            var events = this._eventStore.GetUnpublishedEvents();
    //            foreach (var domainEvent in events)
    //            {
    //                this._eventPublisher.Publish(domainEvent);
    //            }

    //            await Task.Delay(1000, cancellationToken); // 定期檢查
    //        }
    //    }
    //}

    //public class MessageQueueDomainEventPublisherWorker : IDomainEventPublisherWorker
    //{
    //    private readonly IDomainEventStore _eventStore;
    //    private readonly IMessageQueueClient _queueClient;

    //    public MessageQueueDomainEventPublisherWorker(IDomainEventStore eventStore, IMessageQueueClient queueClient)
    //    {
    //        this._eventStore = eventStore;
    //        this._queueClient = queueClient;
    //    }

    //    public void Start()
    //    {
    //        var events = this._eventStore.GetUnpublishedEvents();
    //        foreach (var domainEvent in events)
    //        {
    //            var serializedEvent = JsonConvert.SerializeObject(domainEvent);
    //            this._queueClient.Publish(domainEvent.GetType().Name, serializedEvent);
    //        }
    //    }

    //    public void Stop()
    //    {
    //        // 訊息隊列實作通常不需要停止邏輯
    //    }
    //}

}
