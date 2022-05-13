using System;
using System.Device.Gpio;

namespace PoolBoy.IotDevice
{
    /// <summary>
    /// Responsible for controlling the device IOs
    /// </summary>
    internal class IoService : IDisposable
    {

        private readonly GpioController _gpioController;
        private readonly GpioPin _poolPumpPin;
        private readonly GpioPin _chlorinePumpPin;

        /// <summary>
        /// Creates and initializes the ios
        /// </summary>
        /// <param name="poolPumpGpIoId"></param>
        /// <param name="chlorinePumpGpIoId"></param>
        internal IoService(int poolPumpGpIoId, int chlorinePumpGpIoId)
        {
            _gpioController = new GpioController();
            _poolPumpPin = _gpioController.OpenPin(poolPumpGpIoId);
            _poolPumpPin.SetPinMode(PinMode.Output);
            _poolPumpPin.ValueChanged += OnPoolPumpStateChanged;

            _chlorinePumpPin = _gpioController.OpenPin(chlorinePumpGpIoId);
            _chlorinePumpPin.SetPinMode(PinMode.Output);
            _chlorinePumpPin.ValueChanged += OnChlorinePumpStateChanged;

            _poolPumpPin.Write(PinValue.High);
            _chlorinePumpPin.Write(PinValue.High);
        }




        /// <summary>
        /// Gets wether the pool pump is active
        /// </summary>
        internal bool PoolPumpActive { get; private set; }

        /// <summary>
        /// Gets wether the chlorine pump is active
        /// </summary>
        internal bool ChlorinePumpActive { get; private set; }

        /// <summary>
        /// Starts or stops the pool pump
        /// </summary>
        /// <param name="active"></param>
        internal void ChangePoolPumpStatus(bool active)
        {
            _poolPumpPin.Write(active ? PinValue.Low : PinValue.High);
        }

        /// <summary>
        /// Starts or stops the chlorine pump
        /// </summary>
        /// <param name="active"></param>
        internal void ChangeChlorinePumpStatus(bool active)
        {
            _chlorinePumpPin.Write(active ? PinValue.Low : PinValue.High);
        }
        

        private void OnChlorinePumpStateChanged(object sender, PinValueChangedEventArgs e)
        {
            ChlorinePumpActive = (e.ChangeType & PinEventTypes.Falling) != 0;
        }

        private void OnPoolPumpStateChanged(object sender, PinValueChangedEventArgs e)
        {
            PoolPumpActive = (e.ChangeType & PinEventTypes.Falling) != 0;
        }

        /// <summary>
        /// Closes all native resources
        /// </summary>
        public void Dispose()
        {
            _gpioController.ClosePin(_poolPumpPin.PinNumber);
            _gpioController.ClosePin(_chlorinePumpPin.PinNumber);
            _poolPumpPin.Dispose();
            _chlorinePumpPin.Dispose();
            _gpioController.Dispose();
        }
    }
}
