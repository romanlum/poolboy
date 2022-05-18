using System;

namespace PoolBoy.IotDevice.Infrastructure
{
    /// <summary>
    /// Date and Time extension methods
    /// </summary>
    internal static class DateTimeExtension
    {
        /// <summary>
        /// Converts a time string to a datetime using a fixed date to allow comparison
        /// </summary>
        /// <param name="timeString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        internal static DateTime FromTimeString(string timeString)
        {
            var splitted = timeString.Split(':');

            if (splitted.Length > 1)
            {
                if (int.TryParse(splitted[0], out int hour))
                {
                    if (int.TryParse(splitted[1], out int minute))
                    {
                        if (splitted.Length > 2)
                        {
                            if (int.TryParse(splitted[2], out int second))
                            {
                                return new DateTime(2000, 1, 1, hour, minute, second);
                            }

                            throw new ArgumentException("Invalid time string");

                        }

                        return new DateTime(2000, 1, 1, hour, minute, 0);

                    }
                    
                }
            }
            throw new ArgumentException("Invalid time string");
            
        }
    }
}
