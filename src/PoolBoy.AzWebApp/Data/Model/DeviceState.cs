using PoolBoy.PoolBoyWebApp.Data.Model;

namespace PoolBoyWebApp.Data.Model
{
    public class DeviceState
    {
        public ChlorinePumpStatus ChlorinePumpStatus { get; set; }
        public ChlorinePumpConfig ChlorinePumpConfig { get; set; }

        public PoolPumpConfig PoolPumpConfig { get; set; }
        public PoolPumpStatus PoolPumpStatus { get; set; }
    }
}
