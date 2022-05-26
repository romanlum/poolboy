using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoolBoy.AzChlorineSchedulerFunc.Models.Ondilo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PoolBoy.AzChlorineSchedulerFunc.Services
{
    public class OndiloService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private TokenResult accessToken;

        public OndiloService(IConfiguration config, ILogger<OndiloService> logger)
        {
            _configuration = config;
            _logger = logger;
        }

        public async Task<IEnumerable<Recommendation>> GetRecommendationsAsync()
        {
            using (var client = new HttpClient())
            {
                _logger.LogInformation("Fetch Active Recommendations");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {await GetAccessTokenAsync()}");
                var result = await client.GetAsync($"https://interop.ondilo.com/api/customer/v1/pools/{_configuration["ondiloPoolId"]}/recommendations");
                var content = await result.Content.ReadAsStringAsync();
                _logger.LogInformation("Got Result {0}", content);
                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError("Error during Fetch Recomendations: ResultCode = {0} Content = {1}", result.StatusCode, content);
                    throw new InvalidOperationException($"Error during Fetch Recommendations: ResultCode = {result.StatusCode} Content = {content}");
                }
                return JsonSerializer.Deserialize<IEnumerable<Recommendation>>(content);
            }
        }

        public async Task ValidateRecommendationAsync(int recommendationId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {await GetAccessTokenAsync()}");
                var result = await client.PutAsync($"https://interop.ondilo.com/api/customer/v1/pools/{_configuration["ondiloPoolId"]}/recommendations/{recommendationId}", null);
                var content = await result.Content.ReadAsStringAsync();
                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError("Error during Confirm Recomendations: ResultCode = {0} Content = {1}", result.StatusCode, content);
                    throw new InvalidOperationException($"Error during Confirm Recommendations: ResultCode = {result.StatusCode} Content = {content}");
                }
            }
        }

        public async Task<int> GetCurrentOrpValue()
        {
            using (var client = new HttpClient())
            {
                _logger.LogInformation("Fetch Current ORP Measurement");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {await GetAccessTokenAsync()}");
                var result = await client.GetAsync($"https://interop.ondilo.com/api/customer/v1/pools/{_configuration["ondiloPoolId"]}/lastmeasures?types[]=orp");
                var content = await result.Content.ReadAsStringAsync();
                _logger.LogInformation("Got Result {0}", content);

                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError("Error during Fetch Current Orp Value: ResultCode = {0} Content = {1}", result.StatusCode, content);
                    throw new InvalidOperationException($"Error during Fetch Current Orp Value:: ResultCode = {result.StatusCode} Content = {content}");
                }

                var lastMessares = JsonSerializer.Deserialize<IEnumerable<MeasurementResult>>(content);
                var lastOrpMeasurement = lastMessares?.Where(m => m.IsValid && m.DataType == "orp").OrderByDescending(m => m.ValueTime).FirstOrDefault();
                if (lastOrpMeasurement != null)
                {
                    return lastOrpMeasurement.Value;
                }
                throw new InvalidDataException("Unable to Find Measurment for ORP");
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (accessToken == null || accessToken.ExpiresAt <= DateTime.UtcNow)
            {
                using (var client = new HttpClient())
                {
                    var formContent = new FormUrlEncodedContent(new[]
                                                            {
                                                                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                                                                new KeyValuePair<string, string>("refresh_token", _configuration["ondiloToken"]),
                                                                new KeyValuePair<string, string>("client_id", "customer_api")
                                                            });
                    var result = await client.PostAsync("https://interop.ondilo.com/oauth2/token", formContent);
                    var content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        _logger.LogError("Error during Fetch Access Token: ResultCode = {0} Content = {1}", result.StatusCode, content);
                        throw new InvalidOperationException($"Error during Fetch Access Token: ResultCode = {result.StatusCode} Content = {content}");
                    }
                    accessToken = JsonSerializer.Deserialize<TokenResult>(content);
                    accessToken.ExpiresAt = DateTime.UtcNow.AddSeconds(accessToken.ExpiresIn).AddMinutes(-5);
                }
            }
            return accessToken.AccessToken;
        }
    }
}
