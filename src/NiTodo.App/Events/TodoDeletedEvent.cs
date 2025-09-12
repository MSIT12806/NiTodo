using DomainInfra;
using System;

namespace NiTodo.App.Events
{
    public class TodoDeletedEvent : IDomainEvent
    {
        DateTime _OccurredOn = DateTime.UtcNow;
        public DateTime OccurredOn => _OccurredOn;
        public TodoItem TodoItem { get; }
        public TodoDeletedEvent(TodoItem todoItem)
        {
            TodoItem = todoItem;
        }
    }
}
