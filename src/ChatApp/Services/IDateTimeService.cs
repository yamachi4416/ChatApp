using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
