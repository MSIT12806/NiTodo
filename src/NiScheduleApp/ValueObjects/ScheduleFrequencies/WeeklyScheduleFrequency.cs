using NiScheduleApp.ValueObjects.ScheduleFrequencies;
using System;
using System.Collections.Generic;

namespace NiScheduleApp
{
    public class WeeklyScheduleFrequency : ScheduleFrequency
    {
        public HashSet<DayOfWeek> DaysOfWeek = new HashSet<DayOfWeek>();
        public DailyScheduleFrequency dailySchedule;
        public override DateTime? GetNextOccurrence(DateTime from)
        {
            throw new NotImplementedException();
        }
    }
}
