using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoolBoy.PoolBoyAzChlorineSchedulerFunc.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PoolBoy.AzChlorineSchedulerFunc.Services
{
    public class IoTHubService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly RegistryManager _registryManager;

        public IoTHubService(IConfiguration config, ILogger<IoTHubService> logger, RegistryManager registryManager)
        {
            _logger = logger;   
            _configuration = config;
            _registryManager =  registryManager; 
        }

        public async Task ScheduleChlorinePumpStartAsync(int seconds)
        {
            var twin = await _registryManager.GetTwinAsync(_configuration["poolboydeviceId"]);
            var reportedProperties = JsonSerializer.Deserialize<ReportedProperties>(twin.Properties.Reported.ToJson());
            var desiredProperties = JsonSerializer.Deserialize<DesiredProperties>(twin.Properties.Desired.ToJson());

            desiredProperties.ChlorinePumpConfig.runId = reportedProperties.ChlorinePumpStatus.runId + 1;
            desiredProperties.ChlorinePumpConfig.runtime = seconds;
            desiredProperties.ChlorinePumpConfig.enabled = true;
            desiredProperties.PatchId = reportedProperties.LastPatchId + 1;

            twin.Properties.Desired["chlorinePumpConfig"] = new TwinCollection(JsonSerializer.Serialize<ChlorinePumpConfig>(desiredProperties.ChlorinePumpConfig));
            twin.Properties.Desired["patchId"] = desiredProperties.PatchId;

            await _registryManager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);
        }
    }
}
