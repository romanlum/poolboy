// ReSharper disable InconsistentNaming

namespace PoolBoy.IotDevice.Common.Model
{
    public class PoolPumpConfig
    {
        
        public bool enabled { get; set; }

        public Timeslot[] timeslots { get; set; }
     
        public PoolPumpConfig()
        {
            timeslots = new Timeslot[0];
        }


    }
}
