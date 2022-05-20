namespace PoolBoy.IotDevice.Common.Model
{
    public class DisplayData
    {
        public string IpAddress { get; set; }
        public bool HubConnectionState { get; set; }
        public bool PoolPumpActive { get; set; }
        public bool ChlorinePumpActive { get; set; }
        public string PoolPumpStartTime { get; set; }
        public string PoolPumpStopTime { get; set; } 
        public int ChlorinePumpId { get; set; }
        public int ChlorinePumpRuntime { get; set; }
        public string Error { get; set; }
    }
}