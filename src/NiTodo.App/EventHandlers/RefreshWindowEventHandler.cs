using DomainInfra;
using NiTodo.App.Events;

namespace NiTodo.App
{
    public class RefreshWindowEventHandler : IDomainEventHandler
    {
        //TODO: 這可能要跟框架分手，因為 這個類應該是 App Layer 的一部分
        //private readonly ListWindow _listWindow;
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
                || domainEvent is TodoCompletedAfterFiveSecondsEvent;
        }
    }
}