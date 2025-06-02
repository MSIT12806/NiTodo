using System;
using System.Collections.Generic;

namespace NiTodo.App
{
    public class TodoItem
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime? PlannedDate { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
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

        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Tag cannot be empty.", nameof(tag));
            }
            if (!Tags.Contains(tag))
            {
                Tags.Add(tag);
            }
        }

        public void RemoveTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Tag cannot be empty.", nameof(tag));
            }
            Tags.Remove(tag);
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
