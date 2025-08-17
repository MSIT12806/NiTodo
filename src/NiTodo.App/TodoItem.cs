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
    // 新增建立時間；舊資料可能沒有 -> nullable
    public DateTime? CreatedAt { get; set; }
        public IReadOnlyList<string> Tags
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Content))
                    return Array.Empty<string>();

                var parts = Content.Split('-');

                if (parts.Length <= 1)
                    return Array.Empty<string>();

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
        public bool IsExpired()
        {
            DateTime currentTime = DateTime.Now;

            if (PlannedDate.HasValue == false)
            {
                return false;
            }
            return PlannedDate.Value < currentTime;
        }
        public bool WasExpiredBefore(int minute = 10)
        {
            if (IsExpired() == false)
            {
                return false;
            }

            DateTime currentTime = DateTime.Now;
            var timeSpan = currentTime - PlannedDate.Value;
            return timeSpan.TotalMinutes < minute;
        }
        public bool WillExpireInNext(int minute = 10)
        {
            DateTime currentTime = DateTime.Now;

            if (PlannedDate.HasValue == false)
            {
                return false;
            }
            var timeSpan = PlannedDate.Value - currentTime;
            return timeSpan.TotalMinutes <= minute && timeSpan.TotalMinutes > 0;
        }

        public string GetContentWithoutPrefix()
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                return string.Empty;
            }
            var parts = Content.Split('-');
            if (parts.Length <= 1)
            {
                return Content;
            }
            // 返回最後一個部分，這是沒有前綴的內容
            return parts.Last().Trim();
        }
    }
}
