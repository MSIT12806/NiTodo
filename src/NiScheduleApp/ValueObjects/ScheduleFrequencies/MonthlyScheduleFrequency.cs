using NiScheduleApp.ValueObjects.ScheduleFrequencies;
using System;
using System.Collections.Generic;

namespace NiScheduleApp
{
    public class MonthlyScheduleFrequency : ScheduleFrequency
    {
        public HashSet<int> Days = new HashSet<int>();
        public DailyScheduleFrequency dailySchedule;
        public override DateTime? GetNextOccurrence(DateTime from)
        {
            throw new NotImplementedException();
        }
    }
}
