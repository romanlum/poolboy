﻿// ReSharper disable InconsistentNaming
namespace PoolBoy.IotDevice.Model
{
    public class ChlorinePumpStatus
    {
        internal bool active { get; set; }
        internal int runId { get; set; }
        internal long startedAt { get; set; }
    }
}
