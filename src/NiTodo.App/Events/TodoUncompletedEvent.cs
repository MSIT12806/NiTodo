using DomainInfra;
using System;

namespace NiTodo.App.Events
{
    public class TodoUncompletedEvent : IDomainEvent
    {
        public TodoUncompletedEvent()
        {
        }

        public DateTime OccurredOn => _OccurredOn;
        DateTime _OccurredOn = DateTime.UtcNow;
    }
}