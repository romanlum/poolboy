using PoolBoy.PoolBoyAzChlorineSchedulerFunc.Data.Model;

namespace PoolBoy.PoolBoyAzChlorineSchedulerFunc.Data.Model
{
    public class DeviceState
    {
        public ChlorinePumpStatus ChlorinePumpStatus { get; set; }
        public ChlorinePumpConfig ChlorinePumpConfig { get; set; }

        public PoolPumpConfig PoolPumpConfig { get; set; }
        public PoolPumpStatus PoolPumpStatus { get; set; }

        public int PatchId { get; set; }
        public int LastPatchId { get; set; }
    }
}
