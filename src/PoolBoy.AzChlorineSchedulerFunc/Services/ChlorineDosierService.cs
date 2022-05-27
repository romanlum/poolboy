using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoolBoy.AzChlorineSchedulerFunc.Models.Ondilo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolBoy.AzChlorineSchedulerFunc.Services
{
    public class ChlorineDosierService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        public ChlorineDosierService(IConfiguration config, ILogger<ChlorineDosierService> logger)
        {
            _logger = logger;
            _configuration = config;
        }
        private int CalculateRuntimeInSecondsByChlorineInGramm(int chlorine)
        {
            //Todo. Calulate gramm into ms of runtime for injection pump
            //25ml pro minuten
            return (chlorine/25) * 60;
        }
        public async Task<Tuple<int, IEnumerable<Recommendation>>> GetChlorinePumpRuntimeByRecommendationsAndOrp(IEnumerable<Recommendation> recommendations, int orp)
        {
            //Check if Orp is in a valid range --> if orp is too hight don't put chlorine into the pool
            //Todo: Check logic with orp is valid by pool master
            if(orp >= int.Parse(_configuration["maxOrpToPutChlorine"]))
            {
                _logger.LogInformation("ORP {0} is higher than max ORP allowed to Put Chlorine into the Pool", orp);
                return Tuple.Create(0, (new List<Recommendation>()) as IEnumerable<Recommendation>);
            }
            var chlorineRecommendations = recommendations.Where(r => r.Title.StartsWith("Add") && r.Title.Contains("chlorine"));
            var totalAmount = 0;
            foreach(var recommendation in chlorineRecommendations)
            {
                _logger.LogInformation("Found Recommendation: {0}", recommendation);
                totalAmount += int.Parse(new String(recommendation.Title.Where(c => Char.IsDigit(c)).ToArray()));
            }
            _logger.LogInformation("Calculated Total Amount of Chlorine: {0}", totalAmount);
            return Tuple.Create(CalculateRuntimeInSecondsByChlorineInGramm(totalAmount),chlorineRecommendations);
        }
    }
}
