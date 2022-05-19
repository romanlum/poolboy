﻿// ReSharper disable InconsistentNaming
namespace PoolBoy.IotDevice.Common.Model
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