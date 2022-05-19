using System;

namespace PoolBoy.IotDevice.Common.Infrastructure
{
    public interface IDateTimeService
    {
        /// <summary>
        /// Gets the current utc time
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// Returns a date time from a unix time stamp
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        DateTime FromUnixTimeSeconds(long seconds);

        /// <summary>
        /// Gets the unix time in seconds from a date time
        /// </summary>
        /// <param name="curTime"></param>
        /// <returns></returns>
        long ToUnixTimeSeconds(DateTime curTime);
    }
}