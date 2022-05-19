using PoolBoy.IotDevice.Common;
using PoolBoy.IotDevice.Common.Model;

namespace PoolBoy.IotDevice.Test.Mock
{
    internal class DeviceServiceMock:IDeviceService
    {
        public int PatchId { get; set; }
        public ChlorinePumpConfig ChlorinePumpConfig { get; set; }
        public PoolPumpConfig PoolPumpConfig { get; set; }
        public int LastPatchId { get; set; }
        public PoolPumpStatus PoolPumpStatus { get; set; }
        public ChlorinePumpStatus ChlorinePumpStatus { get; set; }
        public string Error { get; set; }
        
        public bool Connect()
        {
            return ConnectResult;
        }

        public bool ConnectResult { get; set; }
        public bool SendReportedPropertiesCalled { get; set; }

        public void SendReportedProperties()
        {
            SendReportedPropertiesCalled = true;
        }
    }
}
