using System;

namespace NiTodo.App
{
    public class TodoItem
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime? CompleteDateTime { get; set; }
        public void Complete()
        {
            CompleteDateTime = DateTime.Now;
        }
        public bool IsCompleted => CompleteDateTime.HasValue;
        public bool CompletedAfterFiveSeconds(DateTime dt)
        {
            if (CompleteDateTime.HasValue == false)
            {
                return false;
            }
            var completeDateTime = CompleteDateTime.Value;
            var timeSpan = dt - completeDateTime;
            return timeSpan.TotalSeconds > 5;
        }

        public void Uncomplete()
        {
            CompleteDateTime = null;
        }
    }
}
