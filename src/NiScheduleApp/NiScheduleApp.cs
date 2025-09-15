using NiScheduleApp.Interfaces;
using NiTodo.App;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NiSchedule.App
{
    public class CreateScheduleRequest
    {
        public string Content { get; set; }

    }

    public class NiScheduleApp
    {
        private IScheduleRepository Repository { get; }
        public NiScheduleApp(IScheduleRepository repository)
        {
            Repository = repository;
        }
        public void CreateSchedule(ScheduleItem item)
        {
            Repository.Add(item);
        }
        public void DeleteSchedule(string id)
        {
            Repository.Delete(id);
        }
        public List<ScheduleItem> GetAllSchedules()
        {
            return Repository.GetAll();
        }
        public void CreateTodoItem(ScheduleItem item, NiTodoApp todoApp)
        {
            //TODO: 這段要加上測試捏
            var datetime = item.ScheduleFrequency.GetNextOccurrence(DateTime.Now);
            if (todoApp.GetTodoItem(new NiTodo.App.Interfaces.TodoItemSearchRequest { PlannedDate = datetime }).Any()==false)
            {
                todoApp.CreateTodo(item.Content, datetime);
            }
        }
        public void ExcuteSchedules(NiTodoApp todoApp)
        {
            var schedules = Repository.GetAll();
            foreach (var item in schedules)
            {
                CreateTodoItem(item, todoApp);
            }
        }
    }
}
