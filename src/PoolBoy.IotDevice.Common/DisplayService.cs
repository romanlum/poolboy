using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Text;
using Iot.Device.Ssd13xx;
using PoolBoy.IotDevice.Common.Model;

namespace PoolBoy.IotDevice.Common
{
    public class DisplayService : IDisplayService
    {
        private Ssd1306 _displayController;

        public DisplayData Data { get; }

        private bool _errorMode = false;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public DisplayService()
        {
            Data = new DisplayData();
        }

        public void Initialize()
        {
            _displayController = new Ssd1306(I2cDevice.Create(
                    new I2cConnectionSettings(1,
                        Ssd1306.DefaultI2cAddress,
                        I2cBusSpeed.FastMode)),
                Ssd13xx.DisplayResolution.OLED128x64);

            _displayController.ClearScreen();
            _displayController.Font = new BasicFont();
            
        }
        public void Render()
        {

            if (string.IsNullOrEmpty(Data.Error))
            {
                if (_errorMode)
                {
                    _displayController.ClearScreen();
                    _errorMode = false;
                }
                _displayController.DrawString(0, 0, "Ip: " + Data.IpAddress);
                _displayController.DrawString(0, 9, "Hub: " + (Data.HubConnectionState ? "Verbunden" : "Fehler   "));
                _displayController.DrawString(0, 18, "Poolpumpe:  " + (Data.PoolPumpActive ? "ein" : "aus"));
                //_displayController.DrawString(0, 27, " " + Data.PoolPumpStartTime + " - " + Data.PoolPumpStopTime);
                _displayController.DrawString(0, 36, "Chlorpumpe: " + (Data.ChlorinePumpActive ? "ein" : "aus"));
                if (Data.ChlorinePumpActive)
                {
                    _displayController.DrawString(0, 45, " Id " + Data.ChlorinePumpId + " -> " + Data.ChlorinePumpRuntime + " s");
                }
                else
                {
                    _displayController.DrawString(0, 45, "                            ");
                }
                _displayController.DrawString(0, 54, Data.DateTime);
            }
            else
            {
                if (!_errorMode)
                {
                    _displayController.ClearScreen();
                    _errorMode = true;
                    
                }
                _displayController.DrawString(0,0,"--- FEHLER ---");
                _displayController.DrawHorizontalLine(0, 9, 128);
                for (int i = 0; i < Math.Ceiling(Data.Error.Length / 16); i++)
                {
                    _displayController.DrawString(0, 11 + (i * 8), Data.Error.Substring(i*16,16));
                }
            }
            
            
            _displayController.Display();

        }
    }
}
