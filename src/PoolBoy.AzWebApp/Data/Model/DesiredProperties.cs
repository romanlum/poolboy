using PoolBoy.PoolBoyWebApp.Data.Model;
using System.Text.Json.Serialization;

namespace PoolBoyWebApp.Data.Model
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
