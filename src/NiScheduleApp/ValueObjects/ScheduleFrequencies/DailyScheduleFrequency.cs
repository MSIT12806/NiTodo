using NiScheduleApp.ValueObjects.ScheduleFrequencies;
using System;

namespace NiScheduleApp
{
    public class DailyScheduleFrequency : ScheduleFrequency
    {
        public DailyScheduleFrequency(int hour, int minute)
        {
            if (hour < 0 || hour > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23.");
            }
            if (minute < 0 || minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minute), "Minute must be between 0 and 59.");
            }
            Hour = hour;
            Minute = minute;
        }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public override DateTime? GetNextOccurrence(DateTime from)
        {
            var next = new DateTime(from.Year, from.Month, from.Day, Hour, Minute, 0);
            if (next <= from)
            {
                next = next.AddDays(1);
            }
            return next;
        }
    }
}
