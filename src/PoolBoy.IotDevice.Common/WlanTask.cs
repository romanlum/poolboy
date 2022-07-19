using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using nanoFramework.Networking;

namespace PoolBoy.IotDevice.Common
{
    /// <summary>
    /// Task for connecting to a wireless lan
    /// </summary>
    public static class WlanTask
    {
        /// <summary>
        /// Ip address 
        /// </summary>
        public static string Ip {get; private set;}


        /// <summary>
        /// Error message if task fails
        /// </summary>
        public static string ErrorMessage { get; private set; }

        public static bool Run()
        {
            CancellationTokenSource cs = new(10000);
            if (WifiNetworkHelper.Status != NetworkHelperStatus.NetworkIsReady)
            {
                WifiNetworkHelper.SetupNetworkHelper(true);
                var success = WifiNetworkHelper.Reconnect(true, token: cs.Token);
                if (!success)
                {
                    // Something went wrong, you can get details with the ConnectionError property:
                    Debug.WriteLine($"Can't connect to the network, error: {WifiNetworkHelper.Status}");
                    ErrorMessage = WifiNetworkHelper.Status.ToString();
                    Ip = string.Empty;
                    return false;
                    
                }
            }

            Ip = NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address;
            return true;
        }

        public static bool Connected => WifiNetworkHelper.Status == NetworkHelperStatus.NetworkIsReady;

        
    }
}
