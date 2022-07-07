using System.Text.Json.Serialization;

namespace PoolBoy.PoolBoyWebApp.Data.Model
{
    public class PoolPumpConfig
    {
        
        public bool enabled { get; set; }
        [JsonPropertyName("timeslots")]
        public IEnumerable<Timeslot> Timeslots { get; set; }

        public override string ToString()
        {
            return $"{nameof(enabled)}: {enabled}, {nameof(Timeslots)}: {Timeslots?.Select(t => t.ToString())?.Aggregate((x,y) => $"{x}, {y}")}";
        }
    }
    public class Timeslot
    {
        public Timeslot()
        {
            startTime = "00:00";
            stopTime = "00:00";
        }

        public string startTime { get; set; }
        public string stopTime { get; set; }
        public override string ToString()
        {
            return $"{nameof(startTime)}: {startTime}, {nameof(stopTime)}: {stopTime}";
        }
    }
}
