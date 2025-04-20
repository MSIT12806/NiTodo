using DomainInfra;
using System;

namespace NiTodo.App
{
    public class TodoCreatedEvent : IDomainEvent
    {
        public TodoItem TodoItem { get; }

        private DateTime _OccurredOn;
        public DateTime OccurredOn => _OccurredOn;

        public TodoCreatedEvent(TodoItem todoItem)
        {
            TodoItem = todoItem;
            _OccurredOn = DateTime.UtcNow;
        }
    }
}
