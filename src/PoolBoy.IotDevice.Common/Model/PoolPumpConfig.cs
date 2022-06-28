// ReSharper disable InconsistentNaming

namespace PoolBoy.IotDevice.Common.Model
{
    public class PoolPumpConfig
    {
        
        public bool enabled { get; set; }
        public string startTime { get; set; }
        public string stopTime { get; set; }

        public PoolPumpConfig()
        {
            startTime = "00:00";
            stopTime = "00:00";
        }
        

        public override string ToString()
        {
            return $"{nameof(enabled)}: {enabled}, {nameof(startTime)}: {startTime}, {nameof(stopTime)}: {stopTime}";
        }


    }
}
