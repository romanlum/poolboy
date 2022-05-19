using System;

namespace PoolBoy.IotDevice.Infrastructure
{
    public interface IDateTimeService
    {
        /// <summary>
        /// Gets the current utc time
        /// </summary>
        DateTime Now { get; }
    }
}