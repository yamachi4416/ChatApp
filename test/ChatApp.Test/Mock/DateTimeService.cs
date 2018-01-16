using System;
using ChatApp.Services;

namespace ChatApp.Test.Mock
{
    public class DateTimeServiceMock : IDateTimeService
    {
        public static DateTimeOffset _Now;
        public DateTimeOffset Now => _Now;
    }
}