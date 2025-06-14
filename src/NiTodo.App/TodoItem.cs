using System;
using System.Collections.Generic;
using System.Linq;

namespace NiTodo.App
{
    public class TodoItem
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime? PlannedDate { get; set; }
        public IReadOnlyList<string> Tags
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Content)) return Array.Empty<string>();

                var parts = Content.Split('-');

                if (parts.Length <= 1) return Array.Empty<string>();

                return parts.Take(parts.Length - 1).ToList();
            }
        }
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

        public void SetContent(string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
            {
                throw new ArgumentException("New content cannot be empty.", nameof(newContent));
            }
            Content = newContent;
        }

        public void SetPlannedDate(DateTime plannedDate)
        {
            if (plannedDate < DateTime.Now)
            {
                throw new ArgumentException("Planned date cannot be in the past.", nameof(plannedDate));
            }
            PlannedDate = plannedDate;
        }
    }
}
