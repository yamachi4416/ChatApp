using System;
using ChatApp.Services;

namespace ChatApp.Test.Mocks
{
    public class DateTimeServiceMock : IDateTimeService
    {
        public static DateTimeOffset _Now;
        public DateTimeOffset Now => _Now;
    }
}