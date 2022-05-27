using PoolBoy.IotDevice.Common.Model;

namespace PoolBoy.IotDevice.Common
{
    public interface IDeviceService
    {
        /// <summary>
        /// Used for synchronizing parameters from server / client
        /// </summary>
        int PatchId { get; }

        /// <summary>
        /// Configuration of the chlorine pump
        /// </summary>
        ChlorinePumpConfig ChlorinePumpConfig { get; }

        /// <summary>
        /// Configures of the pool pump
        /// </summary>
        PoolPumpConfig PoolPumpConfig { get; }

        /// <summary>
        /// Current applied patch on the client
        /// </summary>
        int LastPatchId { get; set; }

        /// <summary>
        /// Current status of the pool pump
        /// </summary>
        PoolPumpStatus PoolPumpStatus { get; }

        /// <summary>
        /// Current status of the chlorine pump
        /// </summary>
        ChlorinePumpStatus ChlorinePumpStatus { get; }


        /// <summary>
        /// Error property for backend
        /// </summary>
        string Error { get; set; }

        /// <summary>
        /// Defines if the connection to the hub is ok
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Connects to iot hub using default retries
        /// </summary>
        /// <returns></returns>
        bool Connect();

        /// <summary>
        /// Sends the reported properties to the server
        /// </summary>
        void SendReportedProperties();

        bool Reconnect();
    }
}