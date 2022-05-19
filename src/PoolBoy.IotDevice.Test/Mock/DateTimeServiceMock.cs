using System;
using PoolBoy.IotDevice.Common.Infrastructure;

namespace PoolBoy.IotDevice.Test.Mock
{
    internal class DateTimeServiceMock:IDateTimeService
    {
        public DateTime Now { get; set; }
        public DateTime FromUnixTimeSeconds(long seconds)
        {
            return DateTime.FromUnixTimeSeconds(seconds);
        }
    }
}
