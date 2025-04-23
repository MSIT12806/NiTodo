using DomainInfra;
using System;

namespace NiTodo.App
{
    public class TodoUncompletedEvent : IDomainEvent
    {
        public TodoUncompletedEvent(TodoItem todo)
        {
            Todo = todo;
        }
        public TodoItem Todo { get; }

        public DateTime OccurredOn => _OccurredOn;
        DateTime _OccurredOn = DateTime.UtcNow;
    }
}