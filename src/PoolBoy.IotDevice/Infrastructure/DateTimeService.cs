using System;
using System.Collections.Generic;
using System.Text;

namespace PoolBoy.IotDevice.Infrastructure
{
    public class DateTimeService : IDateTimeService
    {
        /// <summary>
        /// Gets the current utc time
        /// </summary>
        public DateTime Now => DateTime.UtcNow;
    }
}
