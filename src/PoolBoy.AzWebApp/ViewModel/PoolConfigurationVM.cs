using PoolBoyWebApp.Data;
using PoolBoyWebApp.Data.Model;
using System.Globalization;

namespace PoolBoyWebApp.ViewModel
{
    public class PoolConfigurationVM : BaseVM, IDisposable
    {
        private readonly IoTHubService _hubService;
        private  DeviceState _deviceState;

        public PoolConfigurationVM(IoTHubService hubService)
        {
            _hubService = hubService;
        }
        public TimeOnly PoolPumpStartTime { get; set; }
        public TimeOnly PoolPumpStopTime { get; set; }
        public int PatchId { get; set; }
        public int LastPatchId { get; set; }

        public int ChlorinePumpDuration { get; set; }
        public bool? ChlorinePumpRunning { get; set; }
        public int ChlorinePumpRunId { get; set; }
        public string EventStream { get; set; }

        public async Task InitializeAsync()
        {
            _deviceState = await _hubService.GetCurrentDeviceState();
            _hubService.DeviceStateChanged += StateChangeHandlerListener;
            _hubService.EventStreamChanged += EventStreamChangedHandlerListener;
            PoolPumpStartTime = TimeOnly.Parse(_deviceState.PoolPumpConfig.startTime);
            PoolPumpStopTime = TimeOnly.Parse(_deviceState.PoolPumpConfig.stopTime);
            PatchId = _deviceState.PatchId;
            LastPatchId = _deviceState.LastPatchId;
            ChlorinePumpDuration = _deviceState.ChlorinePumpConfig.runtime;
            ChlorinePumpRunId = _deviceState.ChlorinePumpConfig.runId;
            ChlorinePumpRunning = _deviceState.ChlorinePumpStatus?.active;
            OnPropertyChanged(nameof(PoolPumpStartTime));

        }

        private void EventStreamChangedHandlerListener(object? sender, string e)
        {
            EventStream += $"NEW EVENT: \n{e}";
            OnPropertyChanged(nameof(EventStream));
        }

        private async void StateChangeHandlerListener(object sender, DeviceState deviceState)
        {
            Console.WriteLine("Device State Changed");
            _deviceState = deviceState;
            PoolPumpStartTime = TimeOnly.Parse(_deviceState.PoolPumpConfig.startTime);
            PoolPumpStopTime = TimeOnly.Parse(_deviceState.PoolPumpConfig.stopTime);
            PatchId = _deviceState.PatchId;
            LastPatchId = _deviceState.LastPatchId;
            ChlorinePumpDuration = _deviceState.ChlorinePumpConfig.runtime;
            ChlorinePumpRunId = _deviceState.ChlorinePumpConfig.runId;
            ChlorinePumpRunning = _deviceState.ChlorinePumpStatus?.active;
            OnPropertyChanged(nameof(PoolPumpStartTime));
        }
        public void Dispose()
        {
            _hubService.DeviceStateChanged -= StateChangeHandlerListener;
        }
    
        public async Task StartChlorinePump()
        {
            _hubService.StartChlorinePump(LastPatchId, ChlorinePumpRunId, ChlorinePumpDuration);
        }
        public async Task StopChlorinePump()
        {
            _hubService.StopChlorinePump(ChlorinePumpRunId, LastPatchId);
        }

        public async Task UpdatePoolPumpConfiguration()
        {
            await _hubService.SetPoolPumpConfiguration(LastPatchId, new PoolBoy.PoolBoyWebApp.Data.Model.PoolPumpConfig()
            {
                enabled = true,
                startTime = PoolPumpStartTime.ToString(CultureInfo.InvariantCulture),
                stopTime = PoolPumpStopTime.ToString(CultureInfo.InvariantCulture)
            }) ;
        }
    }
}
