using System;

namespace NiScheduleApp.ValueObjects.ScheduleFrequencies
{
    public abstract class ScheduleFrequency
    {
        public abstract DateTime? GetNextOccurrence(DateTime from);
    }
}
