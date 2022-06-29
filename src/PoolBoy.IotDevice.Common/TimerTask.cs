using System;
using System.Diagnostics;
using System.Threading;
using PoolBoy.IotDevice.Common.Infrastructure;

namespace PoolBoy.IotDevice.Common
{
    /// <summary>
    /// Task for timer based implementation
    /// </summary>
    public class TimerTask
    {

        private const int LoopTime = 100;
        private const int MaxChlorineRelayTime = 5;

        private readonly IDeviceService _deviceService;
        private readonly IIoService _ioService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDisplayService _displayService;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="deviceService"></param>
        /// <param name="ioService"></param>
        /// <param name="dateTimeService"></param>
        /// <param name="displayService"></param>
        public TimerTask(IDeviceService deviceService, IIoService ioService, IDateTimeService dateTimeService, IDisplayService displayService)
        {
            _deviceService = deviceService;
            _ioService = ioService;
            _dateTimeService = dateTimeService;
            _displayService = displayService;
        }

        /// <summary>
        /// Runs the task in an infinite loop
        /// </summary>
        public void RunLoop(CancellationToken token = default(CancellationToken))
        {
            while(!token.IsCancellationRequested)
            {

                try
                {
#if NANOFRAMEWORK_1_0
                    if (!WlanTask.Connected)
                    {
                        WlanTask.Run();
                    }
#endif
                    
                    if (!_deviceService.Connected)
                    {
                        try
                        {
                            _deviceService.Reconnect();
                        }
                        catch (Exception)
                        {
                            
                        }
                    } 
                    
                    UpdateStatus();
                    
                    
                    
                    _displayService.Data.ChlorinePumpActive = _ioService.ChlorinePumpActive;
                    _displayService.Data.PoolPumpActive = _ioService.PoolPumpActive;
                    //var startTime = DateTimeExtension.FromTimeString(_deviceService.PoolPumpConfig.startTime);
                    //var stopTime = DateTimeExtension.FromTimeString(_deviceService.PoolPumpConfig.stopTime);
                    //_displayService.Data.PoolPumpStartTime = $"{(startTime.Hour + 2).ToString("00")}:{startTime.Minute.ToString("00")}";
                    //_displayService.Data.PoolPumpStopTime = $"{(stopTime.Hour + 2).ToString("00")}:{stopTime.Minute.ToString("00")}";
                    _displayService.Data.ChlorinePumpId = _deviceService.ChlorinePumpConfig.runId;
                    _displayService.Data.ChlorinePumpRuntime = _deviceService.ChlorinePumpConfig.runtime;
                    _displayService.Data.Error = null;
                    _displayService.Data.HubConnectionState = _deviceService.Connected;
                    _displayService.Data.DateTime = DateTime.UtcNow.ToString();
#if NANOFRAMEWORK_1_0
                    _displayService.Data.IpAddress = WlanTask.Connected ? WlanTask.Ip : WlanTask.ErrorMessage;
#endif
                    _displayService.Render();

                }
                catch (Exception e)
                {
                    _displayService.Data.Error = e.Message;
                    // disable pumps on error
                    _ioService.ChangePoolPumpStatus(false);
                    _ioService.ChangeChlorinePumpStatus(false);
                    _displayService.Render();

                }
                finally
                {
                }
                
                Thread.Sleep(LoopTime);

            }
        }

        /// <summary>
        /// Fetches the current parameter and updates the pump status accordingly
        /// </summary>
        public void UpdateStatus()
        {
            try
            {
                var curTime = _dateTimeService.Now;
                bool statusChanged = false;

                //chlorine pump handling
                if (_deviceService.ChlorinePumpConfig.enabled)
                {
                    //chlorine pump should be enabled
                    if (_deviceService.ChlorinePumpConfig.runId > _deviceService.ChlorinePumpStatus.runId && _deviceService.ChlorinePumpConfig.runtime > 0)
                    {
                        statusChanged = true;
                        SetPoolPumpStatus(true);
                        SetChlorinePumpStatus(true);
                        _deviceService.ChlorinePumpStatus.runId = _deviceService.ChlorinePumpConfig.runId;
                        _deviceService.ChlorinePumpStatus.startedAt = _dateTimeService.ToUnixTimeSeconds(curTime);

                    }
                    else if (_deviceService.ChlorinePumpConfig.runId <= _deviceService.ChlorinePumpStatus.runId) //running or already finished
                    {
                        var chlorineEndTime = _dateTimeService.FromUnixTimeSeconds(_deviceService.ChlorinePumpStatus.startedAt).AddSeconds(_deviceService.ChlorinePumpConfig.runtime);
                        if (curTime < chlorineEndTime)
                        {
                            statusChanged = SetPoolPumpStatus(true);
                            if (SetChlorinePumpStatus(true))
                            {
                                statusChanged = true;
                            }
                        }
                        else //finished
                        {
                            statusChanged = SetChlorinePumpStatus(false);
                        }
                    }

                    if (statusChanged)
                    {
                        _deviceService.SendReportedProperties();
                    }

                }
                else
                {
                    statusChanged = SetChlorinePumpStatus(false);
                    if (_deviceService.ChlorinePumpStatus.runId != _deviceService.ChlorinePumpConfig.runId)
                    {
                        _deviceService.ChlorinePumpStatus.runId = _deviceService.ChlorinePumpConfig.runId;
                        statusChanged = true;
                    }
  
                    if (statusChanged)
                    {
                        _deviceService.SendReportedProperties();
              
                    }
                }


                //only change pool pump if chlorine pump is not active
                if (!_deviceService.ChlorinePumpStatus.active)
                {
                    bool shouldRun = false;
                    if(_deviceService.PoolPumpConfig.timeslots != null)
                    {
                        foreach (var slot in _deviceService.PoolPumpConfig.timeslots)
                        {
                            var startTime = DateTimeExtension.FromTimeString(slot.startTime);
                            var stopTime = DateTimeExtension.FromTimeString(slot.stopTime);
                            var checkTime = new DateTime(2000, 01, 01, curTime.Hour, curTime.Minute, curTime.Second);

                            if (_deviceService.PoolPumpConfig.enabled && checkTime >= startTime && checkTime <= stopTime) //should be running
                            {
                                shouldRun = true;
                                break;
                            }

                        }
                    }
                    
                    statusChanged = SetPoolPumpStatus(shouldRun);
                                        

                    //reset error
                    if (_deviceService.Error != null)
                    {
                        _deviceService.Error = null;
                        statusChanged = true;
                    }

                }

                if (_deviceService.LastPatchId != _deviceService.PatchId)
                {
                    _deviceService.LastPatchId = _deviceService.PatchId;
                    statusChanged = true;
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

                throw;
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
                }
                
                if (!_deviceService.PoolPumpStatus.active)
                {
                    
                    _deviceService.PoolPumpStatus.active = true;
                    return true;
                }
            }
            else
            {
                if(_ioService.PoolPumpActive)
                {
                    _ioService.ChangePoolPumpStatus(false);
                }
                if (_deviceService.PoolPumpStatus.active)
                {
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
                }
                
                if (!_deviceService.ChlorinePumpStatus.active)
                {
                    
                    _deviceService.ChlorinePumpStatus.active = true;
                    return true;
                }
            }
            else
            {
                if (_ioService.ChlorinePumpActive)
                {
                    _ioService.ChangeChlorinePumpStatus(false);
                }
                
                if (_deviceService.ChlorinePumpStatus.active)
                {
                    
                    _deviceService.ChlorinePumpStatus.active = false;
                    return true;
                }
            }
            return false;
        }
    }
}
