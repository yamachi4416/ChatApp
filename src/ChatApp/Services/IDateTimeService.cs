using System;

namespace ChatApp.Services
{
    public interface IDateTimeService
    {
        DateTimeOffset Now { get; }
    }

    public class DateTimeService : IDateTimeService
    {
        public DateTimeOffset Now { get { return DateTimeOffset.UtcNow;  } }
    }
}
