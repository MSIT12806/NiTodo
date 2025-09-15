using NiSchedule.App;
using System.Collections.Generic;

namespace NiScheduleApp.Interfaces
{
    public interface IScheduleRepository
    {
        void Add(ScheduleItem item);
        void Delete(string id);
        List<ScheduleItem> GetAll();
    }
}