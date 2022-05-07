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
       
       
            GpioController gpioController = new GpioController();

            var pin = gpioController.OpenPin(25);
            pin.SetPinMode(PinMode.Output);
            pin.Write(PinValue.High);

            var ledPin = gpioController.OpenPin(2);
            ledPin.SetPinMode(PinMode.Output);
            ledPin.Write(PinValue.High);

            ssd1306.ClearScreen();
            ssd1306.DrawString(0, 0, "Connecting...");
            var taskResult = WLANTask.Run();
            ssd1306.ClearScreen();

            if(taskResult)
            {
                ssd1306.DrawString(0, 0, WLANTask.Ip);
            }
            else
            {
                ssd1306.DrawString(0, 0, $"Error: {WLANTask.ErrorMessage}");
            }
            ssd1306.Display();
            Thread.Sleep(Timeout.Infinite);
            return;

            string DeviceID =$"poolboy-{Helpers.Helpers.GetMacId()}";
            const string IotBrokerAddress = "poolboyhub.azure-devices.net";
            const string SasKey = "xxxxx";

            const string AzureRootCA = @"-----BEGIN CERTIFICATE-----
MIIDdzCCAl+gAwIBAgIEAgAAuTANBgkqhkiG9w0BAQUFADBaMQswCQYDVQQGEwJJ
RTESMBAGA1UEChMJQmFsdGltb3JlMRMwEQYDVQQLEwpDeWJlclRydXN0MSIwIAYD
VQQDExlCYWx0aW1vcmUgQ3liZXJUcnVzdCBSb290MB4XDTAwMDUxMjE4NDYwMFoX
DTI1MDUxMjIzNTkwMFowWjELMAkGA1UEBhMCSUUxEjAQBgNVBAoTCUJhbHRpbW9y
ZTETMBEGA1UECxMKQ3liZXJUcnVzdDEiMCAGA1UEAxMZQmFsdGltb3JlIEN5YmVy
VHJ1c3QgUm9vdDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKMEuyKr
mD1X6CZymrV51Cni4eiVgLGw41uOKymaZN+hXe2wCQVt2yguzmKiYv60iNoS6zjr
IZ3AQSsBUnuId9Mcj8e6uYi1agnnc+gRQKfRzMpijS3ljwumUNKoUMMo6vWrJYeK
mpYcqWe4PwzV9/lSEy/CG9VwcPCPwBLKBsua4dnKM3p31vjsufFoREJIE9LAwqSu
XmD+tqYF/LTdB1kC1FkYmGP1pWPgkAx9XbIGevOF6uvUA65ehD5f/xXtabz5OTZy
dc93Uk3zyZAsuT3lySNTPx8kmCFcB5kpvcY67Oduhjprl3RjM71oGDHweI12v/ye
jl0qhqdNkNwnGjkCAwEAAaNFMEMwHQYDVR0OBBYEFOWdWTCCR1jMrPoIVDaGezq1
BE3wMBIGA1UdEwEB/wQIMAYBAf8CAQMwDgYDVR0PAQH/BAQDAgEGMA0GCSqGSIb3
DQEBBQUAA4IBAQCFDF2O5G9RaEIFoN27TyclhAO992T9Ldcw46QQF+vaKSm2eT92
9hkTI7gQCvlYpNRhcL0EYWoSihfVCr3FvDB81ukMJY2GQE/szKN+OMY3EU/t3Wgx
jkzSswF07r51XgdIGn9w/xZchMB5hbgF/X++ZRGjD8ACtPhSNzkE1akxehi/oCr0
Epn3o0WC4zxe9Z2etciefC7IpJ5OCBRLbf1wbWsaY71k5h+3zvDyny67G7fyUIhz
ksLi4xaNmjICq44Y3ekQEe5+NauQrz4wlHrQMz2nZQ/1/I6eYs9HRCwBXbsdtTLS
R9I4LtD+gdwyah617jzV/OeBHRnDJELqYzmp
-----END CERTIFICATE-----
";
            DeviceClient azureIoT = new DeviceClient(IotBrokerAddress, DeviceID, SasKey, azureCert: new X509Certificate(AzureRootCA));


            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var result = azureIoT.Open();
                    Debug.WriteLine("result: " + result);
                    if (result)
                    {
                        break;
                    }

                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                  
                }
            }
            
           
          
          
            var twin = azureIoT.GetTwin(new CancellationTokenSource(5000).Token);
            if (twin == null)
            {
                Debug.WriteLine($"Can't get the twins");
                azureIoT.Close();
                for (int i = 0; i < 5; i++)
                {
                    pin.Toggle();
                    Thread.Sleep(2000);
                }
                return;
            }

            Debug.WriteLine($"Twin DeviceID: {twin.DeviceId}, #desired: {twin.Properties.Desired.Count}, #reported: {twin.Properties.Reported.Count}");

            ssd1306.DrawString(0, 10, "Device Twin:" );
            ssd1306.DrawString(0, 20,twin.DeviceId);
            ssd1306.Display();
            azureIoT.SendMessage(DateTime.UtcNow.ToString());
            pin.Toggle();
            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
