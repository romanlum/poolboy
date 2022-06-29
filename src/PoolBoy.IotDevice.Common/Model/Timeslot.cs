using System;
using System.Collections.Generic;
using System.Text;

namespace PoolBoy.IotDevice.Common.Model
{
    public class Timeslot
    {
        public Timeslot()
        {
            startTime = "00:00";
            stopTime = "00:00";
        }

        public string startTime { get; set; }
        public string stopTime { get; set; }
    }
}
