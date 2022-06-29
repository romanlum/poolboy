namespace PoolBoy.PoolBoyAzChlorineSchedulerFunc.Data.Model
{
    public class ChlorinePumpConfig
    {
        public bool enabled { get; set; }
        public int runId { get; set; }
        public int runtime { get; set; }

        public override string ToString()
        {
            return $"{nameof(enabled)}: {enabled}, {nameof(runId)}: {runId}, {nameof(runtime)}: {runtime}";
        }
    }
}
