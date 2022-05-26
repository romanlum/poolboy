using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PoolBoy.AzChlorineSchedulerFunc.Models.Ondilo
{
    public class MeasurementResult
    {
        [JsonPropertyName("data_type")]
        public string DataType { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("value_time")]
        public DateTime ValueTime { get; set; }

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("exclusion_reason")]
        public object ExclusionReason { get; set; }
    }
}
