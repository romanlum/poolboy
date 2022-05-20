using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using PoolBoy.PoolBoyWebApp.Data.Model;
using PoolBoyWebApp.Data.Model;
using System.Text.Json;

namespace PoolBoyWebApp.Data
{
    public class IoTHubService
    {
        RegistryManager _registryManager; 
        private readonly IConfiguration _config;
        public event EventHandler<DeviceState> DeviceStateChanged;
        public event EventHandler<string> EventStreamChanged;

        private readonly EventProcessorClient _eventProcessor;

        public IoTHubService(IConfiguration config)
        {
            _registryManager = RegistryManager.CreateFromConnectionString(config["azureiothubconnectionstring"]);
            _config = config;
            var storageClient = new BlobContainerClient(_config["azurestorageconnectionstring"], "eventprocessorstorage");
            _eventProcessor = new EventProcessorClient(storageClient, EventHubConsumerClient.DefaultConsumerGroupName, _config["azureiothubeventconnectionstring"]);
            _eventProcessor.ProcessEventAsync += ProcessEventHandler;
            _eventProcessor.ProcessErrorAsync += ProcessErrorEventHandler;
            _eventProcessor.StartProcessing();
        }

        private Task ProcessErrorEventHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine("Error During Process event");
            Console.WriteLine(arg.Exception);
            return Task.CompletedTask;
        }

        private async Task ProcessEventHandler(ProcessEventArgs arg)
        {
            await  arg.UpdateCheckpointAsync(); 
            var rawEventData = arg.Data.EventBody.ToString();
            Console.Write("Got Event:");
            Console.Write(rawEventData);
            EventStreamChanged?.Invoke(this, rawEventData);
            Console.WriteLine("Updated Reported Properties");
            DeviceStateChanged?.Invoke(this, await GetCurrentDeviceState());
            
        }

        public async Task StartChlorinePump(int lastPatchId, int runId, int runtime)
        {
            var twin = await _registryManager.GetTwinAsync(_config["poolboydeviceId"]);
            var desiredProperties = JsonSerializer.Deserialize<DesiredProperties>(twin.Properties.Desired.ToJson());
            desiredProperties.ChlorinePumpConfig.runId = runId + 1;
            desiredProperties.ChlorinePumpConfig.runtime = runtime;
            desiredProperties.ChlorinePumpConfig.enabled = true;
            desiredProperties.PatchId = lastPatchId + 1;

            twin.Properties.Desired["chlorinePumpConfig"] = new TwinCollection(JsonSerializer.Serialize<ChlorinePumpConfig>(desiredProperties.ChlorinePumpConfig));
            twin.Properties.Desired["patchId"] = desiredProperties.PatchId;

            await _registryManager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);
        }

        public async Task StopChlorinePump(int runId, int lastPatchId)
        {
            var twin = await _registryManager.GetTwinAsync(_config["poolboydeviceId"]);
            var desiredProperties = JsonSerializer.Deserialize<DesiredProperties>(twin.Properties.Desired.ToJson());
            desiredProperties.ChlorinePumpConfig.runId = runId;
            desiredProperties.ChlorinePumpConfig.enabled = false;
            desiredProperties.PatchId = lastPatchId + 1;

            twin.Properties.Desired["chlorinePumpConfig"] = new TwinCollection(JsonSerializer.Serialize<ChlorinePumpConfig>(desiredProperties.ChlorinePumpConfig));
            twin.Properties.Desired["patchId"] = desiredProperties.PatchId;

            await _registryManager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);
        }

        public async Task SetPoolPumpConfiguration(int lastPatchId, PoolPumpConfig poolPumpConfig)
        {
            var twin = await _registryManager.GetTwinAsync(_config["poolboydeviceId"]);
            
         
            twin.Properties.Desired["poolPumpConfig"] = new TwinCollection(JsonSerializer.Serialize<PoolPumpConfig>(poolPumpConfig));
            twin.Properties.Desired["patchId"] = lastPatchId +1;

            await _registryManager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);
        }

        public async Task<DeviceState> GetCurrentDeviceState()
        {
            var twin = await _registryManager.GetTwinAsync(_config["poolboydeviceId"]);
            var reportedProperties = JsonSerializer.Deserialize<ReportedProperties>(twin.Properties.Reported.ToJson());
            var desiredProperties = JsonSerializer.Deserialize<DesiredProperties>(twin.Properties.Desired.ToJson());
            return new DeviceState()
            {
                ChlorinePumpConfig = desiredProperties.ChlorinePumpConfig,
                PoolPumpConfig = desiredProperties.PoolPumpConfig,
                ChlorinePumpStatus = reportedProperties.ChlorinePumpStatus,
                PoolPumpStatus = reportedProperties.PoolPumpStatus,
                PatchId = desiredProperties.PatchId,
                LastPatchId = reportedProperties.LastPatchId
            };
        }

    }
}
