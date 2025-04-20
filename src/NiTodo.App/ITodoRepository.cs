using System;
using System.Collections.Generic;
using System.Linq;

namespace NiTodo.App
{
    public interface ITodoRepository
    {
        void Add(TodoItem todoItem);
        List<TodoItem> GetAll();

        List<TodoItem> GetShouldShow();
    }

    public class InMemoryTodoRepository : ITodoRepository
    {
        private List<TodoItem> _todoItems = new List<TodoItem>();

        public void Add(TodoItem todoItem)
        {
            if (todoItem == null)
            {
                throw new System.ArgumentNullException(nameof(todoItem));
            }

            _todoItems.Add(todoItem);
        }

        public List<TodoItem> GetAll()
        {
            return _todoItems.ToList();
        }

        public List<TodoItem> GetShouldShow()
        {
            return _todoItems.Where(t => t.CompletedAfterFiveSeconds(DateTime.Now) == false).ToList();
        }
    }
}
