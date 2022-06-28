using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.Azure.Devices.Shared;
using nanoFramework.Json;
using PoolBoy.IotDevice.Common.Infrastructure;
using PoolBoy.IotDevice.Common.Model;

namespace PoolBoy.IotDevice.Common
{
    /// <summary>
    /// Iot service for communicating with the azure iot hub
    /// </summary>
    public class DeviceService : IDeviceService
    {
        private readonly string _deviceId;
        private readonly string _iotBrokerAddress;
        private readonly string _sasKey;

        /// <summary>
        /// Retries connecting to iot hub
        /// </summary>
        private const int NumberOfRetries = 2;

        /// <summary>
        /// Default timeout for azure calls
        /// </summary>
        private const int DefaultTimeout = 60000;

        /// <summary>
        /// Used for synchronizing parameters from server / client
        /// </summary>
        public int PatchId { get; private set; }

        /// <summary>
        /// Configuration of the chlorine pump
        /// </summary>
        public ChlorinePumpConfig ChlorinePumpConfig { get; private set; }

        /// <summary>
        /// Configures of the pool pump
        /// </summary>
        public PoolPumpConfig PoolPumpConfig { get; private set; }

        /// <summary>
        /// Current applied patch on the client
        /// </summary>
        public int LastPatchId { get; set; }

        /// <summary>
        /// Current status of the pool pump
        /// </summary>
        public PoolPumpStatus PoolPumpStatus { get; private set; }

        /// <summary>
        /// Current status of the chlorine pump
        /// </summary>
        public ChlorinePumpStatus ChlorinePumpStatus { get; private set; }

        /// <summary>
        /// Error property for backend
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Defines if the connection to the hub is ok
        /// </summary>
        public bool Connected => _deviceId != null && _deviceClient.IsConnected;

        private DeviceClient _deviceClient;
        private Twin _deviceTwin;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="iotBrokerAddress"></param>
        /// <param name="sasKey"></param>
        public DeviceService(string deviceId, string iotBrokerAddress, string sasKey)
        {
            _deviceId = deviceId;
            _iotBrokerAddress = iotBrokerAddress;
            _sasKey = sasKey;
            _deviceClient = new DeviceClient(iotBrokerAddress, deviceId, sasKey,
                azureCert: Certificates.AzureRootCertificateAuthority());
            _deviceClient.StatusUpdated += OnStatusUpdated;
            _deviceClient.TwinUpated += OnDeviceTwinUpdated;

            ChlorinePumpStatus = new ChlorinePumpStatus();
            PoolPumpStatus = new PoolPumpStatus();
            Error = null;
        }

        public bool Reconnect()
        {
            _deviceClient.Reconnect();
            return _deviceClient.IsConnected;
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
                    //twin is requested on connected event see OnStatusUpdated
                    return _deviceClient.Open();

                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    // ignored
                }
            }

            return false;
        }

        private void OnStatusUpdated(object sender, StatusUpdatedEventArgs e)
        {
            if(e.IoTHubStatus.Status == Status.Connected)
            {
                CancellationTokenSource cancellationToken = new CancellationTokenSource(DefaultTimeout);
                _deviceTwin = _deviceClient.GetTwin(cancellationToken.Token);
                Debug.WriteLine(_deviceClient.IsConnected + " " + _deviceClient.IoTHubStatus);
                if (_deviceTwin != null)
                {
                    ParseDesiredProperties(_deviceTwin.Properties.Desired);
                    ParseReportedProperties(_deviceTwin.Properties.Reported);
                }
            }
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
            collection.Add(GetJsonName(nameof(Error)), Error);
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
        /// Parses the reported properties
        /// </summary>
        /// <param name="collection"></param>
        private void ParseReportedProperties(TwinCollection collection)
        {
            var propertyName = GetJsonName(nameof(LastPatchId));
            if (collection.Contains(propertyName))
            {
                LastPatchId = int.Parse(collection[propertyName].ToString());
            }

            PoolPumpStatus = DeserializeObject(GetJsonName(nameof(PoolPumpStatus)), collection, typeof(PoolPumpStatus)) as PoolPumpStatus;
            ChlorinePumpStatus = DeserializeObject(GetJsonName(nameof(ChlorinePumpStatus)), collection, typeof(ChlorinePumpStatus)) as ChlorinePumpStatus;
            PoolPumpStatus ??= new PoolPumpStatus();
            ChlorinePumpStatus ??= new ChlorinePumpStatus();
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
