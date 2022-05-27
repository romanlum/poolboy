using PoolBoy.PoolBoyAzChlorineSchedulerFunc.Data.Model;
using System.Text.Json.Serialization;

namespace PoolBoy.PoolBoyAzChlorineSchedulerFunc.Data.Model
{
    public class DesiredProperties
    {
        [JsonPropertyName("patchId")]
        public  int PatchId{ get; set; }
        [JsonPropertyName("poolPumpConfig")]
        public PoolPumpConfig PoolPumpConfig { get; set; }
        [JsonPropertyName("chlorinePumpConfig")]
        public ChlorinePumpConfig ChlorinePumpConfig { get; set; }
    }
}
