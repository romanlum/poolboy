using PoolBoy.PoolBoyAzChlorineSchedulerFunc.Data.Model;
using System.Text.Json.Serialization;

namespace PoolBoy.PoolBoyAzChlorineSchedulerFunc.Data.Model
{
    public class ReportedProperties
    {

        [JsonPropertyName("lastPatchId")]
        public int LastPatchId { get; set; }
        [JsonPropertyName("poolPumpStatus")]
        public PoolPumpStatus PoolPumpStatus { get; set; }
        [JsonPropertyName("chlorinePumpStatus")]
        public ChlorinePumpStatus ChlorinePumpStatus { get; set; }
    }
}
