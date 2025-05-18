using System;

namespace DomainInfra
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
    public class DomainMessage
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
