﻿using System;

namespace PoolBoy.IotDevice.Common
{
    public interface IIoService
    {
        /// <summary>
        /// Gets wether the pool pump is active
        /// </summary>
        bool PoolPumpActive { get; }

        /// <summary>
        /// Gets wether the chlorine pump is active
        /// </summary>
        bool ChlorinePumpActive { get; }

        /// <summary>
        /// Last activation of chlorine pump
        /// </summary>
        DateTime LastChlorinePumpActivation { get; }

        /// <summary>
        /// Starts or stops the pool pump
        /// </summary>
        /// <param name="active"></param>
        void ChangePoolPumpStatus(bool active);

        /// <summary>
        /// Starts or stops the chlorine pump
        /// </summary>
        /// <param name="active"></param>
        void ChangeChlorinePumpStatus(bool active);
    }
}