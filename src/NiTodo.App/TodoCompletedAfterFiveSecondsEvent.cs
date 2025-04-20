using DomainInfra;
using System;

namespace NiTodo.App
{
    public class TodoCompletedAfterFiveSecondsEvent : IDomainEvent
    {
        DateTime _OccurredOn = DateTime.UtcNow;
        public DateTime OccurredOn => _OccurredOn;
        public TodoItem TodoItem { get; }
        public TodoCompletedAfterFiveSecondsEvent(TodoItem todoItem)
        {
            TodoItem = todoItem;
        }
    }
}
