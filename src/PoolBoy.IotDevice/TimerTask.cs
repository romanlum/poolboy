using System;
using System.Diagnostics;
using System.Threading;
using PoolBoy.IotDevice.Infrastructure;

namespace PoolBoy.IotDevice
{
    /// <summary>
    /// Task for timer based implementation
    /// </summary>
    internal class TimerTask
    {

        private const int LoopTime = 100;

        private readonly IDeviceService _deviceService;
        private readonly IIoService _ioService;
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="deviceService"></param>
        /// <param name="ioService"></param>
        public TimerTask(IDeviceService deviceService, IIoService ioService, IDateTimeService dateTimeService)
        {
            _deviceService = deviceService;
            _ioService = ioService;
            _dateTimeService = dateTimeService;
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
                    var curTime = _dateTimeService.Now;
                    bool statusChanged = false;
                    
                    //chlorine pump handling
                    if(_deviceService.ChlorinePumpConfig.enabled)
                    {
                        //chlorine pump should be enabled
                        if(_deviceService.ChlorinePumpConfig.runId > _deviceService.ChlorinePumpStatus.runId && _deviceService.ChlorinePumpConfig.runtime > 0)
                        {
                            statusChanged = SetPoolPumpStatus(true);
                            if(SetChlorinePumpStatus(true))
                            {
                                statusChanged = true;
                            }
                            _deviceService.ChlorinePumpStatus.runId = _deviceService.ChlorinePumpConfig.runId;
                            _deviceService.ChlorinePumpStatus.startedAt = curTime.ToUnixTimeSeconds();

                        } 
                        else if(_deviceService.ChlorinePumpConfig.runId <= _deviceService.ChlorinePumpStatus.runId) //running or already finished
                        {
                            var chlorineEndTime = DateTime.FromUnixTimeSeconds(_deviceService.ChlorinePumpStatus.startedAt).AddSeconds(_deviceService.ChlorinePumpConfig.runtime); 
                            if(curTime < chlorineEndTime) 
                            {
                                statusChanged = SetPoolPumpStatus(true);
                                if(SetChlorinePumpStatus(true))
                                {
                                    statusChanged = true;
                                }
                            }
                            else //finished
                            {
                                statusChanged = SetChlorinePumpStatus(false);
                            }
                        }
                        if(statusChanged)
                        {
                            _deviceService.SendReportedProperties();
                        }
                       
                    }

                    //only change pool pump if chlorine pump is not active
                    if(!_deviceService.ChlorinePumpStatus.active)
                    {
                        var startTime = DateTimeExtension.FromTimeString(_deviceService.PoolPumpConfig.startTime);
                        var stopTime = DateTimeExtension.FromTimeString(_deviceService.PoolPumpConfig.stopTime);
                        var checkTime = new DateTime(2000, 01, 01, curTime.Hour, curTime.Minute, curTime.Second);

                        if (_deviceService.PoolPumpConfig.enabled && checkTime >= startTime && checkTime <= stopTime) //should be running
                        {
                            statusChanged = SetPoolPumpStatus(true);
                        }
                        else
                        {
                            statusChanged = SetPoolPumpStatus(false);
                        }

                        if (_deviceService.LastPatchId != _deviceService.PatchId)
                        {
                            _deviceService.LastPatchId = _deviceService.PatchId;
                            statusChanged = true;
                        }

                        //reset error
                        if (_deviceService.Error != null)
                        {
                            _deviceService.Error = null;
                            statusChanged = true;
                        }
  
                    }

                    if (statusChanged)
                    {
                        _deviceService.SendReportedProperties();
                    }

                }
                catch (Exception e)
                {
                    SetChlorinePumpStatus(false);
                    SetPoolPumpStatus(false);

                    if (_deviceService.Error == null || !_deviceService.Error.Equals(e.Message))
                    {
                        _deviceService.Error = e.Message;
                        _deviceService.SendReportedProperties();
                    }
                }

                Thread.Sleep(LoopTime);

            }
        }

        /// <summary>
        /// Sets the pool pump status and returns if the status had to be changed
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        internal bool SetPoolPumpStatus(bool enabled)
        {
            if(enabled)
            {
                if (!_ioService.PoolPumpActive)
                {
                    _ioService.ChangePoolPumpStatus(true);
                    _deviceService.PoolPumpStatus.active = true;
                    return true;
                }
            }
            else
            {
                if (_ioService.PoolPumpActive)
                {
                    _ioService.ChangePoolPumpStatus(false);
                    _deviceService.PoolPumpStatus.active = false;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the chlorine pump status and returns if the status had to be changed
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        internal bool SetChlorinePumpStatus(bool enabled)
        {
            if (enabled)
            {
                if (!_ioService.ChlorinePumpActive)
                {
                    _ioService.ChangeChlorinePumpStatus(true);
                    _deviceService.ChlorinePumpStatus.active = true;
                    return true;
                }
            }
            else
            {
                if (_ioService.ChlorinePumpActive)
                {
                    _ioService.ChangeChlorinePumpStatus(false);
                    _deviceService.ChlorinePumpStatus.active = false;
                    return true;
                }
            }
            return false;
        }
    }
}
