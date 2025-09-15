using NiScheduleApp.ValueObjects.ScheduleFrequencies;
using System;
using System.Collections.Generic;

namespace NiScheduleApp
{
    public class YearlyScheduleFrequency : ScheduleFrequency
    {
        public HashSet<int> Months = new HashSet<int>();
        public MonthlyScheduleFrequency monthlySchedule;
        public override DateTime? GetNextOccurrence(DateTime from)
        {
            throw new NotImplementedException();
        }
    }
}
