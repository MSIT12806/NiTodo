﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NiTodo.App
{
    public interface ITodoRepository
    {
        void Add(TodoItem todoItem);
        IEnumerable<TodoItem> GetAll();
        void SaveChange(TodoItem item);
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

        public IEnumerable<TodoItem> GetAll()
        {
            return _todoItems.ToList();
        }


        public void SaveChange(TodoItem item)
        {
            var oriItem = _todoItems.First(i => i.Id == item.Id);
            var index = _todoItems.IndexOf(oriItem);
            _todoItems[index] = item;
        }
    }

    public class FileTodoRepository : ITodoRepository
    {
        List<TodoItem> _todoItems = new List<TodoItem>();
        readonly string _filePath = Path.Combine("data", "todo.txt");
        public FileTodoRepository()
        {
            // 讀取檔案內容
            if (System.IO.File.Exists(_filePath))
            {
                ReadFromFile();
            }
        }
        public void Add(TodoItem todoItem)
        {
            // 將 todoItem 儲存到檔案的邏輯
            if (todoItem == null)
            {
                throw new System.ArgumentNullException(nameof(todoItem));
            }
            _todoItems.Add(todoItem);
            WriteToFile();
        }
        public IEnumerable<TodoItem> GetAll()
        {
            return _todoItems;
        }

        public void SaveChange(TodoItem item)
        {
            var oriItem = _todoItems.First(i => i.Id == item.Id);
            var index = _todoItems.IndexOf(oriItem);
            _todoItems[index] = item;
            WriteToFile();
        }


        private void ReadFromFile()
        {
            // 將 file 內容全部讀取到 _todoItems
            var json = System.IO.File.ReadAllText(_filePath);
            _todoItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TodoItem>>(json);
        }
        private void WriteToFile()
        {
            // 確保資料夾存在
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // 將 _todoItems 轉換成 json 字串，然後存進檔案當中
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_todoItems);
            System.IO.File.WriteAllText(_filePath, json);
        }
    }
}
