using DomainInfra;
using NiTodo.App;

namespace Tests
{
    public class TodoTests
    {
        TodoService todoService;
        DomainEventDispatcher domainEventDispatcher;
        [SetUp]
        public void Setup()
        {
            domainEventDispatcher = new DomainEventDispatcher();
            todoService = new TodoService(new InMemoryTodoRepository(), domainEventDispatcher);
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
            Assert.That(todos[0].CompletedAfterFiveSeconds(DateTime.Now.AddSeconds(5)), Is.True);
        }

        //TODO: 測試 service.GetAllTodos() 
        //TODO: 測試 serevice.ShouldShow()
    }

    internal class TodoCreatedEventTestHandler : IDomainEventHandler
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