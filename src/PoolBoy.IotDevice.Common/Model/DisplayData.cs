namespace PoolBoy.IotDevice.Common.Model
{
    public class DisplayData
    {
        public string IpAddress { get; set; }
        public bool HubConnectionState { get; set; }
        public bool PoolPumpActive { get; set; }
        public bool ChlorinePumpActive { get; set; }
        public int ChlorinePumpId { get; set; }
        public int ChlorinePumpRuntime { get; set; }
        public string Error { get; set; }

        public string DateTime { get; set; }
    }
}