using System;

namespace PoolBoy.IotDevice.Common.Infrastructure
{
    public class DateTimeService : IDateTimeService
    {
        /// <summary>
        /// Gets the current utc time
        /// </summary>
        public DateTime Now => DateTime.UtcNow;

        /// <summary>
        /// Returns a date time from a unix time stamp
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public DateTime FromUnixTimeSeconds(long seconds)
        {

            return DateTime.FromUnixTimeSeconds(seconds);


        }

        public long ToUnixTimeSeconds(DateTime curTime)
        {
            return curTime.ToUnixTimeSeconds();
        }
    }
}
