using DomainInfra;
using NiTodo.App.Events;
using NiTodo.App.Interfaces;

namespace NiTodo.App.EventHandlers
{
    public class RefreshWindowEventHandler : IDomainEventHandler
    {
        private readonly IUiRenderer _listWindow;
        public RefreshWindowEventHandler(IUiRenderer listWindow)
        {
            _listWindow = listWindow;
        }
        public void Handle(IDomainEvent domainEvent)
        {
            _listWindow.Render();
        }
        public bool IsThisEvent(IDomainEvent domainEvent)
        {
            return domainEvent is TodoCreatedEvent
                || domainEvent is TodoCompletedEvent
                || domainEvent is TodoCompletedAfterFiveSecondsEvent
                || domainEvent is TodoUncompletedEvent
                ;
        }
    }
}