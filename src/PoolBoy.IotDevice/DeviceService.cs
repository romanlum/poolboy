using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.Azure.Devices.Shared;
using nanoFramework.Json;
using PoolBoy.IotDevice.Infrastructure;
using PoolBoy.IotDevice.Model;

namespace PoolBoy.IotDevice
{
    /// <summary>
    /// Iot service for communicating with the azure iot hub
    /// </summary>
    internal class DeviceService
    {
        /// <summary>
        /// Retries connecting to iot hub
        /// </summary>
        private const int NumberOfRetries = 10;

        /// <summary>
        /// Default timeout for azure calls
        /// </summary>
        private const int DefaultTimeout = 15000;

        /// <summary>
        /// Used for synchronizing parameters from server / client
        /// </summary>
        internal int PatchId { get; private set; }

        /// <summary>
        /// Configuration of the chlorine pump
        /// </summary>
        internal ChlorinePumpConfig ChlorinePumpConfig { get; private set; }

        /// <summary>
        /// Configures of the pool pump
        /// </summary>
        internal PoolPumpConfig PoolPumpConfig { get; private set; }

        /// <summary>
        /// Current applied patch on the client
        /// </summary>
        internal int LastPatchId { get; private set; }

        /// <summary>
        /// Current status of the pool pump
        /// </summary>
        internal PoolPumpStatus PoolPumpStatus { get; }

        /// <summary>
        /// Current status of the chlorine pump
        /// </summary>
        internal ChlorinePumpStatus ChlorinePumpStatus { get; }

        private readonly DeviceClient _deviceClient;
        private Twin _deviceTwin;

        public DeviceService(string deviceId, string iotBrokerAddress, string sasKey)
        {
            _deviceClient = new DeviceClient(iotBrokerAddress, deviceId, sasKey,
                azureCert: Certificates.AzureRootCertificateAuthority);
        }

        /// <summary>
        /// Connects to iot hub using default retries
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            
            for (int i = 0; i < NumberOfRetries; i++)
            {
                try
                {
                    var result = _deviceClient.Open();
                    if (result)
                    {
                        CancellationTokenSource cancellationToken = new CancellationTokenSource(DefaultTimeout);
                        _deviceTwin = _deviceClient.GetTwin(cancellationToken.Token);
                        Debug.WriteLine(_deviceClient.IsConnected + " " + _deviceClient.IoTHubStatus);
                        if(_deviceTwin != null)
                        {
                            _deviceClient.TwinUpated += OnDeviceTwinUpdated;
                            ParseDesiredProperties(_deviceTwin.Properties.Desired);
                            return true;
                        }
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(1000);
                    Debug.WriteLine(ex.ToString());
                    // ignored
                }
            }

            return false;
        }

        /// <summary>
        /// Sends the reported properties to the server
        /// </summary>
        public void SendReportedProperties()
        {
            var collection = new TwinCollection();
            collection.Add(GetJsonName(nameof(LastPatchId)), LastPatchId);
            collection.Add(GetJsonName(nameof(PoolPumpStatus)), PoolPumpStatus);
            collection.Add(GetJsonName(nameof(ChlorinePumpStatus)), ChlorinePumpStatus);
            _deviceClient.UpdateReportedProperties(collection);
        }
                

        /// <summary>
        /// Parses the desired properties
        /// </summary>
        /// <param name="collection"></param>
        private void ParseDesiredProperties(TwinCollection collection)
        {
            var propertyName = GetJsonName(nameof(PatchId));
            if (collection.Contains(propertyName))
            {
                PatchId = int.Parse(collection[propertyName].ToString());
            }

            PoolPumpConfig = DeserializeObject(GetJsonName(nameof(PoolPumpConfig)), collection,typeof(PoolPumpConfig)) as PoolPumpConfig;
            ChlorinePumpConfig = DeserializeObject(GetJsonName(nameof(ChlorinePumpConfig)), collection, typeof(ChlorinePumpConfig)) as ChlorinePumpConfig;
        }

        /// <summary>
        /// Converts a property name to a json name (camelCase)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string GetJsonName(string input)
        {
            return input.Substring(0, 1).ToLower() + input.Substring(1);
        }

        /// <summary>
        /// Deserializes an object from a device twin collection by serializing an deserializing the value :p
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="collection"></param>
        /// <param name="resultType"></param>
        /// <returns></returns>
        private object DeserializeObject(string propertyName, TwinCollection collection, Type resultType)
        {
            if (!collection.Contains(propertyName))
            {
                return null;
            }
            var str = JsonConvert.SerializeObject(collection[propertyName]);
            return JsonConvert.DeserializeObject(str, resultType);
        }

        
        /// <summary>
        /// Device twin changed on backend
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeviceTwinUpdated(object sender, TwinUpdateEventArgs e)
        {
            ParseDesiredProperties(e.Twin);
        }

    }
}
