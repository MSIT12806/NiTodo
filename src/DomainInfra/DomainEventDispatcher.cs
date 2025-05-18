using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainInfra
{
    /// <summary>
    /// 領域事件分發器，負責接收領域事件，並將其分發到事件處理器，當然也可以是 DomainEventPublisher
    /// </summary>
    public class DomainEventDispatcher
    {
        private static readonly ConcurrentBag<IDomainEventHandler> _eventHandlers = new ConcurrentBag<IDomainEventHandler>();
        //private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10); // 控制最大併發數

        public void Register(IDomainEventHandler eventHandler)
        {
            _eventHandlers.Add(eventHandler);
        }

        public void Dispatch<TEvent>(IEnumerable<TEvent> events, Func<Task> callback) where TEvent : IDomainEvent
        {
            foreach (var domainEvent in events)
            {
                this.Dispatch(domainEvent, callback);
            }
        }
        public void Dispatch<TEvent>(TEvent domainEvent, Func<Task> callback) where TEvent : IDomainEvent
        {
            foreach (var handler in _eventHandlers)
            {
                if (handler.IsThisEvent(domainEvent))
                {
                    // 往外部發送事件，不阻塞當前執行緒
                    Task.Run(() =>
                    {
                        this.HandleEventAsync(handler, domainEvent);
                    }
                    );
                }
            }

            if (callback != null)
            {
                callback();
            }
        }

        private void HandleEventAsync<TEvent>(IDomainEventHandler handler, TEvent domainEvent) where TEvent : IDomainEvent
        {
            try
            {
                handler.Handle(domainEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handler failed: {ex.Message}");
            }
        }
    }

}
