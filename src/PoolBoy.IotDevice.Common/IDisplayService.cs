using PoolBoy.IotDevice.Common.Model;

namespace PoolBoy.IotDevice.Common
{
    public interface IDisplayService
    {
        DisplayData Data { get; }
        void Initialize();
        void Render();
    }
}