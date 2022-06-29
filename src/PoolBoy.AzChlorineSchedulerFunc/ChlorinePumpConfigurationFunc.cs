using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PoolBoy.AzChlorineSchedulerFunc.Services;

namespace PoolBoy.AzChlorineSchedulerFunc
{
    public class ChlorinePumpConfigurationFunc
    {
        private readonly ILogger _logger;
        private readonly ChlorineDosierService _doesierService;
        private readonly IoTHubService _hubService;
        private readonly OndiloService _ondiloService;


        public ChlorinePumpConfigurationFunc(ILoggerFactory loggerFactory, IoTHubService hubService, OndiloService ondiloService, ChlorineDosierService dosierService)
        {
            _logger = loggerFactory.CreateLogger<ChlorinePumpConfigurationFunc>();
            _hubService = hubService;
            _ondiloService = ondiloService;
            _doesierService = dosierService;
        }
        //0 0 6-20 * * * 
        [Function("ChlorinePumpConfigurationFunc")]
        public async Task Run([TimerTrigger("1 * * * * *")] MyInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var recommendations = await _ondiloService.GetRecommendationsAsync();
            var orpValue = await _ondiloService.GetCurrentOrpValue();
            _logger.LogInformation($"Got Recommendations {recommendations} and OprValue {orpValue}");
            if (recommendations!= null && recommendations.Any())
            {
                var chlorineToSchedule = await _doesierService.GetChlorinePumpRuntimeByRecommendationsAndOrp(recommendations, orpValue);
                if (chlorineToSchedule.Item1 > 0)
                {
                    _logger.LogInformation($"Got Chlorine to Schedule in ms {chlorineToSchedule.Item1}");
                    try
                    {
                        await _hubService.ScheduleChlorinePumpStartAsync(chlorineToSchedule.Item1);
                    }catch(Exception e)
                    {
                        _logger.LogError(e, $"Schedule in ms failed {e.Message}");
                    }
                    foreach(var recommondation in chlorineToSchedule.Item2)
                    {
                        _logger.LogInformation($"Recommendation to Validate {recommondation.Id} {recommondation.Title} {recommondation.Message}");
                        await _ondiloService.ValidateRecommendationAsync(recommondation.Id);
                        _logger.LogInformation($"Recommondation {recommondation.Id} validated");
                    }
                }
                else
                {
                    _logger.LogInformation("Got nothing to Schedule");

                }
            }
            else
            {
                _logger.LogInformation("No Recommendations");
            }
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
