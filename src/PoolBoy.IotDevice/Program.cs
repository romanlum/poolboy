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


            var width = 16;
            var height = 16;
            var ssd1306 = new Ssd1306(
                I2cDevice.Create(
                    new I2cConnectionSettings(
                        1,
                        Ssd1306.DefaultI2cAddress,
                        I2cBusSpeed.FastMode)),
                Ssd13xx.DisplayResolution.OLED128x64);

            ssd1306.ClearScreen();
            ssd1306.Font = new BasicFont();


           


            ssd1306.ClearScreen();
            ssd1306.DrawString(0, 0, "Connecting...");
            var taskResult = WlanTask.Run();
            ssd1306.ClearScreen();

            if(taskResult)
            {
                ssd1306.DrawString(0, 0, WlanTask.Ip);
            }
            else
            {
                ssd1306.DrawString(0, 0, $"Error: {WlanTask.ErrorMessage}");
            }
           
            ssd1306.Display();

            //IoService service = new IoService(25, 26);
            //Thread.Sleep(5000);
            //ssd1306.DrawString(0,10,"PoolPump: " +service.PoolPumpActive.ToString());
            //ssd1306.DrawString(0, 20, "ChlorinePump: " + service.ChlorinePumpActive.ToString());

            //ssd1306.Display();
            //Thread.Sleep(2000);
            //service.ChangePoolPumpStatus(true);
            //service.ChangeChlorinePumpStatus(true);
            //Thread.Sleep(2000);

            //ssd1306.DrawString(0, 10, "PoolPump: " + service.PoolPumpActive.ToString());
            //ssd1306.DrawString(0, 20, "ChlorinePump: " + service.ChlorinePumpActive.ToString());

            //ssd1306.Display();

            //Thread.Sleep(3000);
            //service.ChangePoolPumpStatus(false);
            //service.ChangeChlorinePumpStatus(false);
            //Thread.Sleep(1000);

            //ssd1306.DrawString(0, 10, "PoolPump: " + service.PoolPumpActive.ToString());
            //ssd1306.DrawString(0, 20, "ChlorinePump: " + service.ChlorinePumpActive.ToString());

            //ssd1306.Display();

          
            string DeviceID =$"poolboy-{Helpers.Helpers.GetMacId()}";
            const string IotBrokerAddress = "poolboyhub.azure-devices.net";
            const string SasKey = "6QJ3Oh1c4ObtvJIjMW8FYYbQI4ndnG1m9cACg4+deHg=";

            var service = new DeviceService(DeviceID, IotBrokerAddress, SasKey);
            ssd1306.DrawString(0,10,"Connecting to hub...");
            ssd1306.Display();
            if (service.Connect())
            {
                ssd1306.DrawString(0, 10, "Connected to hub");
            }
            else
            {
                ssd1306.DrawString(0, 10, "failed to connect.");
            }
            ssd1306.Display();
            TimerTask timer = new TimerTask(service, new IoService(25, 26), new DateTimeService());
            timer.RunLoop();
            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
