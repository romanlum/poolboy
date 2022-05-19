using PoolBoy.IotDevice.Common;

namespace PoolBoy.IotDevice.Test.Mock
{
    internal class IoServiceMock:IIoService
    {
        public bool PoolPumpActive { get; set; }
        public bool ChlorinePumpActive { get; set; }
        public void ChangePoolPumpStatus(bool active)
        {
            PoolPumpActive = active;
        }

        public void ChangeChlorinePumpStatus(bool active)
        {
            ChlorinePumpActive = active;
        }
    }
}
