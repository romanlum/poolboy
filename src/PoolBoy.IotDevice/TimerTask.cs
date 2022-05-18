using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PoolBoy.IotDevice
{
    internal class TimerTask
    {

        private const int LoopTime = 5000;

        private readonly DeviceService deviceService;
        private readonly IoService ioService;

        public TimerTask(DeviceService deviceService, IoService ioService)
        {
            this.deviceService = deviceService;
            this.ioService = ioService;
        }

        internal void RunLoop()
        {
            while(true)
            {

                var startTime = DateTime.Parse("2000-01-01 " + deviceService.PoolPumpConfig.startTime);
                var stopTime = DateTime.Parse("2000-01-01 " + deviceService.PoolPumpConfig.stopTime);
                var curTime =  DateTime.UtcNow;
                var checkTime = new DateTime(2000, 01, 01, curTime.Hour, curTime.Minute, curTime.Second);

                bool statusChanged = false;
                if(startTime >= checkTime && checkTime <= stopTime)
                {
                    if (!ioService.PoolPumpActive)
                    {
                        statusChanged = true;
                    }
                    ioService.ChangePoolPumpStatus(true);
                    deviceService.PoolPumpStatus.active = true;
                }
                else
                {
                    if (ioService.PoolPumpActive)
                    {
                        statusChanged = true;
                    }
                    ioService.ChangePoolPumpStatus(false);
                    deviceService.PoolPumpStatus.active = false;
                }

                if(deviceService.LastPatchId != deviceService.PatchId)
                {
                    deviceService.LastPatchId = deviceService.PatchId;
                    statusChanged = true;
                }
                
                if(statusChanged)
                {
                    deviceService.SendReportedProperties();
                }
                

                Thread.Sleep(5000);

            }
        }
    }
}
