namespace PoolBoy.PoolBoyAzChlorineSchedulerFunc.Data.Model
{
    public class PoolPumpConfig
    {
        
        public bool enabled { get; set; }
        public string startTime { get; set; }
        public string stopTime { get; set; }

        public override string ToString()
        {
            return $"{nameof(enabled)}: {enabled}, {nameof(startTime)}: {startTime}, {nameof(stopTime)}: {stopTime}";
        }
    }
}
