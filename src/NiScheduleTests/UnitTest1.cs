using DomainInfra;
using NiSchedule.App;
using NiScheduleApp;
using NiScheduleApp.Interfaces;
using NiTodo.App;
using NiTodo.App.Interfaces;

namespace NiScheduleTests
{
    public class InMemoryScheduleRepository : IScheduleRepository
    {
        private readonly List<ScheduleItem> _items = new List<ScheduleItem>();
        public void Add(ScheduleItem item)
        {
            _items.Add(item);
        }
        public void Delete(string id)
        {
            var item = _items.Find(i => i.Id == id);
            if (item != null)
            {
                _items.Remove(item);
            }
        }
        public List<ScheduleItem> GetAll()
        {
            return new List<ScheduleItem>(_items);
        }
    }
    public class Tests
    {
        NiTodoApp todoService;
        DomainEventDispatcher domainEventDispatcher;
        NiSchedule.App.NiScheduleApp scheduleApp;
        [SetUp]
        public void Setup()
        {
            domainEventDispatcher = new DomainEventDispatcher();
            todoService = new NiTodoApp(new InMemoryTodoRepository(), domainEventDispatcher, null);
            scheduleApp = new NiSchedule.App.NiScheduleApp(new InMemoryScheduleRepository());
        }

        [Test]
        public void CreateTodoItem_ShouldGetOnlyOneData()
        {
            var now = DateTime.Now;
            // Arrange
            var schedule = new ScheduleItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "Test Schedule",
                ScheduleFrequency = new DailyScheduleFrequency(now.Hour, now.Minute)
            };
            scheduleApp.CreateSchedule(schedule);
            // Act
            scheduleApp.ExcuteSchedules(todoService);
            // Assert
            var todos = todoService.GetAllTodos();
            Assert.That(todos.Count, Is.EqualTo(1));
        }
        [Test]
        public void CreateTodoItem_ShouldGetOnlyOneData_WhenDataExist()
        {
            var now = DateTime.Now;
            // Arrange
            var schedule = new ScheduleItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "Test Schedule",
                ScheduleFrequency = new DailyScheduleFrequency(now.Hour, now.Minute)
            };
            scheduleApp.CreateSchedule(schedule);

            todoService.CreateTodo("Test Schedule", now.AddDays(1));
            // Act
            scheduleApp.ExcuteSchedules(todoService);
            // Assert
            var todos = todoService.GetAllTodos();
            Assert.That(todos.Count, Is.EqualTo(1));
        }
    }
}