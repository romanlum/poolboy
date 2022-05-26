using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolBoy.AzChlorineSchedulerFunc.Services
{
    public class IoTHubService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        public IoTHubService(IConfiguration config, ILogger<IoTHubService> logger)
        {
            _logger = logger;   
            _configuration = config;
        }

        public async Task ScheduleChlorinePumpStartAsync(int seconds)
        {
            throw new NotImplementedException();
        }
    }
}
