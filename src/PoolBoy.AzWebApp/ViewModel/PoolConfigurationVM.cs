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
        public IList<Tuple<TimeOnly,TimeOnly>> TimeSlots { get; set; } = new List<Tuple<TimeOnly,TimeOnly>>();
       
        public int PatchId { get; set; }
        public int LastPatchId { get; set; }

        public int ChlorinePumpDuration { get; set; }
        public bool? ChlorinePumpRunning { get; set; }
        public bool? PoolPumpRunning { get; set; }
        public int ChlorinePumpRunId { get; set; }
        public string EventStream { get; set; }

        public async Task InitializeAsync()
        {
            _deviceState = await _hubService.GetCurrentDeviceState();
            _hubService.DeviceStateChanged += StateChangeHandlerListener;
            _hubService.EventStreamChanged += EventStreamChangedHandlerListener;
            TimeSlots = _deviceState.PoolPumpConfig.Timeslots.Select(x => Tuple.Create(TimeOnly.Parse(x.startTime), TimeOnly.Parse(x.stopTime))).ToList();
            
            PatchId = _deviceState.PatchId;
            LastPatchId = _deviceState.LastPatchId;
            ChlorinePumpDuration = _deviceState.ChlorinePumpConfig.runtime;
            ChlorinePumpRunId = _deviceState.ChlorinePumpConfig.runId;
            ChlorinePumpRunning = _deviceState.ChlorinePumpStatus?.active;
            PoolPumpRunning = _deviceState.PoolPumpStatus?.active;
            OnPropertyChanged(nameof(TimeSlots));

        }

        private void EventStreamChangedHandlerListener(object? sender, string e)
        {
            EventStream += $"NEW EVENT: \n{e}";
            OnPropertyChanged(nameof(EventStream));
        }

        public async void AddNewTimeSlot()
        {
            TimeSlots.Add(Tuple.Create(TimeOnly.FromDateTime(DateTime.UtcNow), TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1))));
        }
        public async void RemoveTimeSlot(int index)
        {
            TimeSlots.RemoveAt(index);
        }

        private async void StateChangeHandlerListener(object sender, DeviceState deviceState)
        {
            Console.WriteLine("Device State Changed");
            _deviceState = deviceState;
            TimeSlots = _deviceState.PoolPumpConfig.Timeslots.Select(x => Tuple.Create(TimeOnly.Parse(x.startTime), TimeOnly.Parse(x.stopTime))).ToList();

            PatchId = _deviceState.PatchId;
            LastPatchId = _deviceState.LastPatchId;
            ChlorinePumpDuration = _deviceState.ChlorinePumpConfig.runtime;
            ChlorinePumpRunId = _deviceState.ChlorinePumpConfig.runId;
            ChlorinePumpRunning = _deviceState.ChlorinePumpStatus?.active;
            PoolPumpRunning = _deviceState.PoolPumpStatus?.active;
            OnPropertyChanged(nameof(TimeSlots));
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
                Timeslots = TimeSlots.Select(t => new PoolBoy.PoolBoyWebApp.Data.Model.Timeslot() { startTime = t.Item1.ToString(CultureInfo.InvariantCulture), stopTime = t.Item2.ToString(CultureInfo.InvariantCulture)})
            }) ;
        }
    }
}
