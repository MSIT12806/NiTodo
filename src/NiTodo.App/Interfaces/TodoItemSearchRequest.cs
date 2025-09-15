using System;

namespace NiTodo.App.Interfaces
{
    public class TodoItemSearchRequest
    {
        public string ContentKeyword { get; set; }
        public DateTime? PlannedDate { get; set; }
        public string Tag { get; set; }
        public TodoStatus? Status { get; set; }
        public bool IncludeCompleted { get; set; } = false;
    }
}