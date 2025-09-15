using System;
using System.Collections.Generic;
using System.Linq;

namespace NiTodo.App
{
    public enum TodoStatus
    {
        None,
        //開發相關
        待開發,
        待測試,
    }
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
        public TodoStatus Status
        {
            get
            {
                // 狀態就是Content字串中，第一個用"["和"]"夾起來的文字。ex: 國教署開發-[待開發]報名系統， status = 待開發
                var statusStart = Content.IndexOf('[');
                var statusEnd = Content.IndexOf(']');
                if (statusStart >= 0 && statusEnd > statusStart)
                {
                    var statusString = Content.Substring(statusStart + 1, statusEnd - statusStart - 1);
                    if (Enum.TryParse<TodoStatus>(statusString, out var status))
                    {
                        return status;
                    }
                    else
                    {
                        return TodoStatus.None;
                    }
                }

                return TodoStatus.None;
            }
        }
        public DateTime? CompleteDateTime { get; set; }
        public void Complete()
        {
            if(this.IsCompleted)
            {
                return;
            }
            if(this.Status != TodoStatus.None)
            {
                // 如果有設定狀態，則把狀態往下一個移動
                var nextStatus = this.Status + 1;
                if (Enum.IsDefined(typeof(TodoStatus), nextStatus) && nextStatus != TodoStatus.None)
                {
                    // 只要不是None，就更新
                    var tags = this.Tags;
                    var contentWithoutPrefix = GetContentWithoutPrefix();
                    this.Content = string.Join("-", tags) + (tags.Any() ? "-" : "") + $"[{nextStatus}]" + contentWithoutPrefix;

                    return;
                }
            }
            CompleteDateTime = DateTime.Now;
        }
        public bool IsCompleted => CompleteDateTime.HasValue;
        public bool HasCompletedFiveSecondsBefore(DateTime dt)
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
            // 1. 拿掉 前綴標籤 (tags)
            // 2. 拿掉 狀態標籤 [待開發]、[待測試] 等

            var parts = Content.Split('-');
            var contentPart = parts.LastOrDefault()?.Trim() ?? string.Empty;
            var statusStart = contentPart.IndexOf('[');
            var statusEnd = contentPart.IndexOf(']');
            if (statusStart >= 0 && statusEnd > statusStart)
            {
                // 有狀態標籤，拿掉
                contentPart = contentPart.Remove(statusStart, statusEnd - statusStart + 1).Trim();
            }

            return contentPart;
        }
    }

}
