﻿using DomainInfra;
using NiTodo.App.Events;
using NiTodo.App.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NiTodo.App
{
    public class NiTodoApp
    {
        public TodoItem SelectedTodoItem;
        public SortMode CurrentSortMode = SortMode.Content;
        public HashSet<string> IncludeTags = new HashSet<string>();
        public HashSet<string> ExcludeTags = new HashSet<string>();
        public bool ShowCompletedItems = false;
        public bool ShowTodayItems = false;

        private readonly ITodoRepository _todoRepository;
        private readonly DomainEventDispatcher _domainEventDispatcher;
        private readonly ICopyContent copyContent;

        public NiTodoApp(
            ITodoRepository todoRepository,
            DomainEventDispatcher domainEventDispatcher,
            ICopyContent copyContent
            )
        {
            _todoRepository = todoRepository;
            _domainEventDispatcher = domainEventDispatcher;
            this.copyContent = copyContent;
        }

        public void CancelCompleteTodo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Todo ID cannot be empty.", nameof(id));
            }
            var todoItem = _todoRepository.GetAll().FirstOrDefault(t => t.Id == id);
            if (todoItem == null)
            {
                throw new KeyNotFoundException($"Todo with ID {id} not found.");
            }
            todoItem.Uncomplete();
            _todoRepository.SaveChange(todoItem);

            // 發出領域事件
            var todoUncompletedEvent = new TodoUncompletedEvent();
            _domainEventDispatcher.Dispatch(todoUncompletedEvent, null);
        }

        public void CompleteTodo(string id)
        {
            TodoItem todoItem = GetItem(id);
            todoItem.Complete();
            _todoRepository.SaveChange(todoItem);
            // 發出領域事件
            var todoCompletedEvent = new TodoCompletedEvent(todoItem);
            _domainEventDispatcher.Dispatch(todoCompletedEvent, null);

            //註冊一個五秒後的領域事件
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);

                var fiveSecondsLaterEvent = new TodoCompletedAfterFiveSecondsEvent(todoItem);
                _domainEventDispatcher.Dispatch(fiveSecondsLaterEvent, null);
            });
        }

        private TodoItem GetItem(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Todo ID cannot be empty.", nameof(id));
            }
            var todoItem = _todoRepository.GetAll().FirstOrDefault(t => t.Id == id);
            if (todoItem == null)
            {
                throw new KeyNotFoundException($"Todo with ID {id} not found.");
            }

            return todoItem;
        }

        public string CreateTodo(string todoContent, DateTime? plannedDatetime=null)
        {
            if (string.IsNullOrWhiteSpace(todoContent))
            {
                throw new ArgumentException("Todo content cannot be empty.", nameof(todoContent));
            }
            //去除掉多餘的空白
            todoContent = TrimWhiteSpace(todoContent);
            var todoItem = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = todoContent,
                CreatedAt = DateTime.Now,
                PlannedDate = plannedDatetime
            };

            _todoRepository.Add(todoItem);

            // 發出領域事件
            var todoCreatedEvent = new TodoCreatedEvent(todoItem);
            _domainEventDispatcher.Dispatch(todoCreatedEvent, null);

            return todoItem.Id;
        }

        private string TrimWhiteSpace(string todoContent)
        {
            //1.標籤的銜接符`-`前後空白 應該去除
            //2.狀態標籤`[]`前後空白 應該去除
            //ex: 標籤A-[狀態A]文字內容 文字內容

            todoContent = TrimWithCharacter(todoContent,'-');
            todoContent = TrimWithCharacter(todoContent, '[');
            todoContent = TrimWithCharacter(todoContent, ']');
            return todoContent;
        }

        private string TrimWithCharacter(string todoContent, char v)
        {
            var parts = todoContent.Split(v);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }
            return string.Join(v.ToString(), parts);
        }

        public List<TodoItem> GetAllTodos()
        {
            return _todoRepository.GetAll().ToList();
        }
        public IEnumerable<TodoItem> GetTodoItem(TodoItemSearchRequest req)
        {
            var r = _todoRepository.GetAll();
            if (req.IncludeCompleted == false)
            {
                r = r.Where(t => t.HasCompletedFiveSecondsBefore(DateTime.Now) == false);
            }

            if (!string.IsNullOrWhiteSpace(req.ContentKeyword))
            {
                r = r.Where(t => t.Content.IndexOf(req.ContentKeyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (req.PlannedDate.HasValue)
            {
                r = r.Where(t => t.PlannedDate.HasValue && t.PlannedDate.Value.Date == req.PlannedDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(req.Tag))
            {
                r = r.Where(t => t.Tags.Contains(req.Tag));
            }

            if (req.Status.HasValue)
            {
                r = r.Where(t => t.Status == req.Status.Value);
            }

            return r;
        }

        public List<TodoItem> ShowTodo()
        {
            var r = _todoRepository.GetAll();

            if (ShowCompletedItems == false)
            {
                r = r
                .Where(t => t.HasCompletedFiveSecondsBefore(DateTime.Now) == false);
            }

            if (ShowTodayItems)
            {
                r = r
                .Where(t => (t.PlannedDate.HasValue == false) ||
                (t.PlannedDate.HasValue && t.PlannedDate.Value.Date == DateTime.Now.Date));
            }

            if (IncludeTags.Any())
                r = r.Where(i => i.Tags.Any(t => IncludeTags.Contains(t)));
            if (ExcludeTags.Any())
                r = r.Where(i => !i.Tags.Any(t => ExcludeTags.Contains(t)));

            return r.ToList();
        }

        public void UpdateTodo(TodoItem todo)
        {
            _todoRepository.SaveChange(todo);
        }

        public void CopyContent(TodoItem todo)
        {
            var content = todo.GetContentWithoutPrefix();
            copyContent.Copy(content);
        }

        public void SetTagInclude(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Tag cannot be empty.", nameof(tag));
            }
            ExcludeTags.Remove(tag);
            IncludeTags.Add(tag);
        }

        public void SetTagExclude(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Tag cannot be empty.", nameof(tag));
            }
            IncludeTags.Remove(tag);
            ExcludeTags.Add(tag);
        }

        public void SetTagIgnore(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException("Tag cannot be empty.", nameof(tag));
            }
            IncludeTags.Remove(tag);
            ExcludeTags.Remove(tag);
        }

        public IEnumerable<string> GetAllTags()
        {
            var items = _todoRepository.GetAll();

            if (ShowCompletedItems == false)
            {
                items = _todoRepository.GetAll()
                .Where(t => t.HasCompletedFiveSecondsBefore(DateTime.Now) == false);
            }

            return items.SelectMany(t => t.Tags)
                 .Distinct()
                 .OrderBy(t => t);
        }

        public TagFilterState GetTagFilterState(string tag)
        {
            if (IncludeTags.Contains(tag))
            {
                return TagFilterState.Include;
            }
            else if (ExcludeTags.Contains(tag))
            {
                return TagFilterState.Exclude;
            }
            else
            {
                return TagFilterState.Ignore;
            }
        }

        public void ResetTagLists()
        {
            var tags = GetAllTags();
            IncludeTags.IntersectWith(tags);
            ExcludeTags.IntersectWith(tags);
        }

        public void DeleteTodo(string id)
        {
            var todoItem = GetItem(id);
            _todoRepository.Delete(todoItem);
            // 發出領域事件
            var todoDeletedEvent = new TodoDeletedEvent(todoItem);
            _domainEventDispatcher.Dispatch(todoDeletedEvent, null);
        }

        public void RestoreTodo(TodoItem deleted)
        {
            if (deleted == null) throw new ArgumentNullException(nameof(deleted));

            // 重新加入原本刪除的項目（保留其 Id 與內容）
            _todoRepository.Add(deleted);

            // 觸發 UI 更新（可視為重新建立事件）
            var todoCreatedEvent = new TodoCreatedEvent(deleted);
            _domainEventDispatcher.Dispatch(todoCreatedEvent, null);
        }
    }
    public enum SortMode
    {
        Content,
        Created, // 目前沒有保存建立日期，暫以 Id 生成順序 (Guid) 代替，或保持原順序
        Planned
    }

    public enum TagFilterState
    {
        Ignore, Include, Exclude
    }

}
