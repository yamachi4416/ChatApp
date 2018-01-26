using System;
using ChatApp.Services;

namespace ChatApp.Test.Mocks
{
    public class DateTimeServiceMock : IDateTimeService
    {
        public DateTimeOffset _Now = default(DateTimeOffset);
        public DateTimeOffset Now => _Now;
    }
}