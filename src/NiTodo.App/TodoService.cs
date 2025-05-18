using DomainInfra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NiTodo.App
{
    public class TodoService
    {
        private readonly ITodoRepository _todoRepository;
        private readonly DomainEventDispatcher _domainEventDispatcher;
        public TodoService(ITodoRepository todoRepository, DomainEventDispatcher domainEventDispatcher)
        {
            _todoRepository = todoRepository;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public void CancelCompleteTodo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Todo ID cannot be empty.", nameof(id));
            }
            var todoItem = _todoRepository.GetAll().Find(t => t.Id == id);
            if (todoItem == null)
            {
                throw new KeyNotFoundException($"Todo with ID {id} not found.");
            }
            todoItem.Uncomplete();
            _todoRepository.SaveChange(todoItem);

            // 發出領域事件
            var todoUncompletedEvent = new TodoUncompletedEvent(todoItem);
            _domainEventDispatcher.Dispatch(todoUncompletedEvent, null);
        }

        public void CompleteTodo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Todo ID cannot be empty.", nameof(id));
            }
            var todoItem = _todoRepository.GetAll().Find(t => t.Id == id);
            if (todoItem == null)
            {
                throw new KeyNotFoundException($"Todo with ID {id} not found.");
            }
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

        public string CreateTodo(string todoContent)
        {
            if (string.IsNullOrWhiteSpace(todoContent))
            {
                throw new ArgumentException("Todo content cannot be empty.", nameof(todoContent));
            }
            var todoItem = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = todoContent
            };
            _todoRepository.Add(todoItem);

            // 發出領域事件
            var todoCreatedEvent = new TodoCreatedEvent(todoItem);
            _domainEventDispatcher.Dispatch(todoCreatedEvent, null);

            return todoItem.Id;
        }

        public List<TodoItem> GetAllTodos()
        {
            return _todoRepository.GetAll();
        }

        public List<TodoItem> ShowTodo()
        {
            return _todoRepository.GetShouldShow();
        }
    }
}
