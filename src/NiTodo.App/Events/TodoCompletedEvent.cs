using DomainInfra;
using System;

namespace NiTodo.App.Events
{
    public class TodoCompletedEvent : IDomainEvent
    {
        DateTime _OccurredOn = DateTime.UtcNow;
        public DateTime OccurredOn => _OccurredOn;
        public TodoItem TodoItem { get; }

        public TodoCompletedEvent(TodoItem todoItem)
        {
            TodoItem = todoItem;
        }
    }
}
