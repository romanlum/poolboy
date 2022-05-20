using System;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.Networking;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Iot.Device.Ssd13xx;
using PoolBoy.IotDevice.Common;
using PoolBoy.IotDevice.Common.Infrastructure;

namespace PoolBoy.IotDevice
{
    public class Program
    {
        public static void Main()
        {


            Debug.WriteLine($"Hello from nanoFramework! {Helpers.Helpers.GetMacId()}");
            DisplayService displayService = new DisplayService();
            displayService.Initialize();
            displayService.Data.IpAddress = "Connecting";
            displayService.Render();
            var taskResult = WlanTask.Run();
            
            if(taskResult)
            {
                displayService.Data.IpAddress = WlanTask.Ip;
            }
            else
            {
                displayService.Data.IpAddress = $"Error: {WlanTask.ErrorMessage}";
            }
           
            displayService.Render();
            string DeviceID =$"poolboy-{Helpers.Helpers.GetMacId()}";
            const string IotBrokerAddress = "poolboyhub.azure-devices.net";
            const string SasKey = "6QJ3Oh1c4ObtvJIjMW8FYYbQI4ndnG1m9cACg4+deHg=";

            var service = new DeviceService(DeviceID, IotBrokerAddress, SasKey);
            if (service.Connect())
            {
                displayService.Data.HubConnectionState = true;
                TimerTask timer = new TimerTask(service, new IoService(25, 26), new DateTimeService(), displayService);
                timer.RunLoop();
                
            }
            else
            {
                displayService.Data.HubConnectionState = false;
            }
            displayService.Render();

            Thread.Sleep(Timeout.Infinite);
            
        }
    }
}
