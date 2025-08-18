using DomainInfra;
using NiTodo.App;
using NiTodo.App.Events;

namespace Tests
{
    public class MockCopyContent : ICopyContent
    {
        public string CopiedContent { get; private set; }
        public void Copy(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Content cannot be empty.", nameof(content));
            }
            CopiedContent = content;
        }
    }
    public class TodoTests
    {
        NiTodoApp todoService;
        DomainEventDispatcher domainEventDispatcher;
        ICopyContent copyContent;
        [SetUp]
        public void Setup()
        {
            domainEventDispatcher = new DomainEventDispatcher();
            copyContent = new MockCopyContent();
            todoService = new NiTodoApp(new InMemoryTodoRepository(), domainEventDispatcher, copyContent);
        }

        [Test]
        public void CreateTodoItem()
        {
            // Arrange
            // 註冊一個事件監聽，監聽 TodoCreatedEvent
            var todoCreatedEventHandler = new TodoCreatedEventTestHandler();
            domainEventDispatcher.Register(todoCreatedEventHandler);

            // Act
            todoService.CreateTodo("Test Todo");

            // Assert
            var todos = todoService.GetAllTodos();
            Assert.That(todos.Count, Is.EqualTo(1));
            Assert.That(todoCreatedEventHandler.isCreated, Is.True, "TodoCreatedEvent should be handled.");
        }

        [Test]
        public void CompleteTodoItem()
        {
            // Arrange
            var id = todoService.CreateTodo("Test Todo");

            // Act
            todoService.CompleteTodo(id);

            // Assert
            var todos = todoService.GetAllTodos();
            Assert.That(todos[0].IsCompleted, Is.True);
        }

        [Test]
        public void CompleteTodoItemBeforeFiveSeconds()
        {
            // Arrange
            var id = todoService.CreateTodo("Test Todo");
            // Act
            todoService.CompleteTodo(id);
            // Assert
            var todos = todoService.GetAllTodos();
            Assert.That(todos[0].HasCompletedFiveSecondsBefore(DateTime.Now.AddSeconds(5)), Is.True);
        }

        //TODO: 測試 service.GetAllTodos() 

        #region ShowTodo

        //// 1. 顯示該顯示的
        //// 2. 不顯示不該顯示的
        [Test]
        public void ShowTodo_should_show_todo_uncompleted()
        {
            //Arrange .. create a uncompleted todo item
            var id = todoService.CreateTodo("Test Todo");
            //Act .. excute ShowTodo
            var todos = todoService.ShowTodo(true, false);
            //Assert
            Assert.That(todos.Count, Is.EqualTo(1));
        }

        [Test]
        public void ShowTodo_should_show_todo_completed()
        {
            //Arrange .. create a completed todo item
            var id = todoService.CreateTodo("Test Todo");
            todoService.CompleteTodo(id);
            //Act .. excute ShowTodo
            var todos = todoService.ShowTodo(false, false);
            //Assert
            Assert.That(todos.Count, Is.EqualTo(1));
        }

        [Test]
        public void ShowTodo_should_not_show_todo_completed_after_five_seconds()
        {
            //Arrange .. create a completed todo item
            var id = todoService.CreateTodo("Test Todo");
            todoService.CompleteTodo(id);
            Thread.Sleep(5001);
            //Act .. excute ShowTodo
            var todos = todoService.ShowTodo(true, false);
            //Assert
            Assert.That(todos.Count, Is.EqualTo(0));
        }
        #endregion
        //測試取消完成代辦事項的功能
        [Test]
        public void CancelCompleteTodoItem()
        {
            // Arrange
            var id = todoService.CreateTodo("Test Todo");
            todoService.CompleteTodo(id);
            // Act
            todoService.CancelCompleteTodo(id);
            // Assert
            var todos = todoService.GetAllTodos();
            Assert.That(todos[0].IsCompleted, Is.False);
        }

        //[Test]
        //public void AddSubTodoItem()
        //{
        //    // Arrange
        //    var id = todoService.CreateTodo("Test Todo");
        //    // Act
        //    var subId = todoService.CreateSubTodo(id, "Test Sub Todo");
        //    // Assert
        //    var todos = todoService.GetAllTodos();
        //    Assert.That(todos[0].SubTodoItems.Count, Is.EqualTo(1));
        //    Assert.That(todos[0].SubTodoItems[0].Id, Is.EqualTo(subId));
        //}

        //[Test]
        //private void CompleteParentTodoItemWhenSubTodoItemNotCompleted_Error()
        //{
        //    // Arrange
        //    var id = todoService.CreateTodo("Test Todo");
        //    var subId = todoService.CreateSubTodo(id, "Test Sub Todo");
        //    // Act
        //    todoService.CompleteTodo(subId);
        //    // Assert
        //    var todos = todoService.GetAllTodos();
        //    Assert.That(todos[0].IsCompleted, Is.False);
        //}
    }

    public class TodoCreatedEventTestHandler : IDomainEventHandler
    {
        public bool isCreated = false;
        public void Handle(IDomainEvent domainEvent)
        {
            isCreated = true;
        }

        public bool IsThisEvent(IDomainEvent domainEvent)
        {
            return domainEvent is TodoCreatedEvent;
        }
    }
}