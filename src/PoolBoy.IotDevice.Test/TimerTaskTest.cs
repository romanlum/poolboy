using nanoFramework.TestFramework;
using System;
using PoolBoy.IotDevice.Common;
using PoolBoy.IotDevice.Common.Model;
using PoolBoy.IotDevice.Test.Mock;

namespace PoolBoy.IotDevice.Test
{
    [TestClass]
    public class Test
    {


        [TestMethod]
        public void TEstMethod()
        {
            var service = new DeviceServiceMock();
            service.ChlorinePumpConfig = new ChlorinePumpConfig();
            service.ChlorinePumpStatus = new ChlorinePumpStatus();
            service.PoolPumpConfig = new PoolPumpConfig();
            service.PoolPumpConfig.startTime = "13:00";
            service.PoolPumpConfig.stopTime = "13:00";
            service.PoolPumpStatus = new PoolPumpStatus();
            
            var task = new TimerTask(service,new IoServiceMock(), new DateTimeServiceMock());
            
                task.UpdateStatus();
                Assert.True(service.SendReportedPropertiesCalled,"hallo");
            
           
            
            
        }
    }
}
