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
            
            var service = new DeviceService(DeviceID, IotBrokerAddress, "xxx");
            try
            {
                service.Connect();
            }
            catch (Exception)
            {
            }

            {
            //    displayService.Data.HubConnectionState = true;
                TimerTask timer = new TimerTask(service, new IoService(25, 26), new DateTimeService(), displayService);
                timer.RunLoop();
                
            }
         
            displayService.Render();

            Thread.Sleep(Timeout.Infinite);
            
        }
    }
}
