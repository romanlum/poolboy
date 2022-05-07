using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using nanoFramework.Networking;

namespace PoolBoy.IotDevice
{
    /// <summary>
    /// Task for connecting to a wireless lan
    /// </summary>
    internal static class WLANTask
    {
        /// <summary>
        /// Ip address 
        /// </summary>
        internal static string Ip {get; private set;}


        /// <summary>
        /// Error message if task fails
        /// </summary>
        internal static string ErrorMessage { get; private set; }

        internal static bool Run()
        {
            CancellationTokenSource cs = new(10000);
            var success = WiFiNetworkHelper.Reconnect(true, token: cs.Token);
            if (!success)
            {
                // Something went wrong, you can get details with the ConnectionError property:
                Debug.WriteLine($"Can't connect to the network, error: {WiFiNetworkHelper.Status}");
                ErrorMessage = WiFiNetworkHelper.Status.ToString();
                Ip = string.Empty;
                return false;
                
            }

            Ip = NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address;
            return true;
        }
    }
}
