// ReSharper disable InconsistentNaming
namespace PoolBoy.IotDevice.Common.Model
{
    public class ChlorinePumpStatus
    {
        public bool active { get; set; }
        public int runId { get; set; }
        public long startedAt { get; set; }
    }
}
