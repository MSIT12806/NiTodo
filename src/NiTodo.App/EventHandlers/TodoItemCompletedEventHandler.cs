using DomainInfra;
using NiTodo.App.Events;
using System;

namespace NiTodo.App.EventHandlers
{
    public class TodoItemCompletedEventHandler : IDomainEventHandler
    {
        private readonly NiTodoApp niTodoApp;
        public TodoItemCompletedEventHandler(NiTodoApp app)
        {
            niTodoApp = app ?? throw new ArgumentNullException(nameof(app));
        }
        public void Handle(IDomainEvent domainEvent)
        {
            niTodoApp.ResetTagLists(); 
        }
        public bool IsThisEvent(IDomainEvent domainEvent)
        {
            return  domainEvent is TodoCompletedAfterFiveSecondsEvent;
        }
    }
}