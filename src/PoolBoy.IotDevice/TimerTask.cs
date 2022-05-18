using System;
using System.Diagnostics;
using System.Threading;
using PoolBoy.IotDevice.Infrastructure;

namespace PoolBoy.IotDevice
{
    internal class TimerTask
    {

        private const int LoopTime = 100;

        private readonly DeviceService _deviceService;
        private readonly IoService _ioService;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="deviceService"></param>
        /// <param name="ioService"></param>
        public TimerTask(DeviceService deviceService, IoService ioService)
        {
            _deviceService = deviceService;
            _ioService = ioService;
        }

        /// <summary>
        /// Runs the task in an infinite loop
        /// </summary>
        internal void RunLoop(CancellationToken token = default(CancellationToken))
        {
            while(!token.IsCancellationRequested)
            {
                try
                {
                    var curTime = DateTime.UtcNow;
                    var startTime = DateTimeExtension.FromTimeString(_deviceService.PoolPumpConfig.startTime);
                    var stopTime = DateTimeExtension.FromTimeString(_deviceService.PoolPumpConfig.stopTime);
                    
                    var checkTime = new DateTime(2000, 01, 01, curTime.Hour, curTime.Minute, curTime.Second);

                    bool statusChanged = false;
                    Debug.WriteLine(_deviceService.PoolPumpConfig.enabled.ToString());
                    if (_deviceService.PoolPumpConfig.enabled && checkTime >= startTime && checkTime <= stopTime)
                    {
                        if (!_ioService.PoolPumpActive)
                        {
                            statusChanged = true;
                        }

                        _ioService.ChangePoolPumpStatus(true);
                        _deviceService.PoolPumpStatus.active = true;
                    }
                    else
                    {
                        if (_ioService.PoolPumpActive)
                        {
                            statusChanged = true;
                        }

                        _ioService.ChangePoolPumpStatus(false);
                        _deviceService.PoolPumpStatus.active = false;
                    }

                    if (_deviceService.LastPatchId != _deviceService.PatchId)
                    {
                        _deviceService.LastPatchId = _deviceService.PatchId;
                        statusChanged = true;
                    }

                    if (_deviceService.Error != null)
                    {
                        _deviceService.Error = null;
                        statusChanged = true;
                    }

                    if (statusChanged)
                    {
                        _deviceService.SendReportedProperties();
                    }
                }
                catch (Exception e)
                {
                    _ioService.ChangeChlorinePumpStatus(false);
                    _ioService.ChangePoolPumpStatus(false);
                    _deviceService.PoolPumpStatus.active = false;
                    _deviceService.ChlorinePumpStatus.active = false;

                    if (_deviceService.Error == null || !_deviceService.Error.Equals(e.Message))
                    {
                        _deviceService.Error = e.Message;
                        _deviceService.SendReportedProperties();
                    }
                }

                Thread.Sleep(LoopTime);

            }
        }
    }
}
