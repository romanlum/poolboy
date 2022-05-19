using Moq;
using PoolBoy.IotDevice.Common;
using PoolBoy.IotDevice.Common.Infrastructure;
using PoolBoy.IotDevice.Common.Model;

namespace PoolBoy.IotDevice.Test
{
    public class TimerTaskTest
    {
        private readonly Mock<IIoService> _ioService;
        private readonly Mock<IDeviceService> _deviceService;
        private readonly Mock<IDateTimeService> _timeService;

        private readonly PoolPumpStatus _poolPumpStatus = new PoolPumpStatus();
        private readonly ChlorinePumpStatus _chlorinePumpStatus = new ChlorinePumpStatus();

        public TimerTaskTest()
        {
            _ioService = new Mock<IIoService>();
            _deviceService = new Mock<IDeviceService>()
                .SetupProperty(x => x.Error, null);
            _deviceService.SetupGet(x => x.PoolPumpStatus).Returns(_poolPumpStatus);
            _deviceService.SetupGet(x => x.ChlorinePumpStatus).Returns(_chlorinePumpStatus);
            _timeService = new Mock<IDateTimeService>();
        }

        [Theory]
        [InlineData(13, 00, "13:00", "13:03", true)]
        [InlineData(13,00,"12:00","13:03",true)]
        [InlineData(13, 00, "12:00", "13:00", true)]
        [InlineData(13, 00, "13:01", "13:03", false)]
        [InlineData(13, 00, "15:00", "13:00", false)]
        
        public void PoolPumpTimeTest(int currentHour, int currentMinute,string start,string end,bool running)
        {
            var task = new TimerTask(_deviceService.Object, _ioService.Object, _timeService.Object);
            var config = new PoolPumpConfig
            {
                startTime = start,
                stopTime = end,
                enabled = true
            };
            var chlorineConfig = new ChlorinePumpConfig()
            {
                runId = 0,
                runtime = 0,
                enabled = true
            };

            _deviceService.Setup(x => x.PoolPumpConfig).Returns(config);
            _deviceService.Setup(x => x.ChlorinePumpConfig).Returns(chlorineConfig);
            _timeService.Setup(x => x.Now).Returns(new DateTime(2020, 1, 1, currentHour, currentMinute, 0));

            if (running)
            {
                _deviceService.Setup(x => x.SendReportedProperties()).Verifiable();
                _ioService.Setup(x => x.ChangePoolPumpStatus(true)).Verifiable();
            }
            

            task.UpdateStatus();
            
            Assert.Equal(running,_poolPumpStatus.active);
                        
            _ioService.Verify();
            _deviceService.Verify();
            Assert.Null(_deviceService.Object.Error);

        }

        [Fact]
        public void PoolPumpDisabledTest()
        {
            var task = new TimerTask(_deviceService.Object, _ioService.Object, _timeService.Object);
            var config = new PoolPumpConfig
            {
                startTime = "12:00",
                stopTime = "14:00",
                enabled = false
            };
            var chlorineConfig = new ChlorinePumpConfig()
            {
                runId = 0,
                runtime = 0,
                enabled = true
            };

            _deviceService.Setup(x => x.PoolPumpConfig).Returns(config);
            _deviceService.Setup(x => x.ChlorinePumpConfig).Returns(chlorineConfig);
            _timeService.Setup(x => x.Now).Returns(new DateTime(2020, 1, 1, 13, 0, 0));

            
            task.UpdateStatus();

            Assert.False(_poolPumpStatus.active);
            

        }

        [Fact]
        public void ChangePoolPumpTimeTest()
        {

            var task = new TimerTask(_deviceService.Object, _ioService.Object, _timeService.Object);
            var config = new PoolPumpConfig
            {
                startTime = "13:00",
                stopTime = "14:00",
                enabled = true
            };
            var chlorineConfig = new ChlorinePumpConfig()
            {
                runId = 0,
                runtime = 0,
                enabled = true
            };

            _deviceService.Setup(x => x.PoolPumpConfig).Returns(config);
            _deviceService.Setup(x => x.ChlorinePumpConfig).Returns(chlorineConfig);
            _timeService.Setup(x => x.Now).Returns(new DateTime(2020, 1, 1, 13, 1, 0));
            _ioService.SetupGet(x => x.PoolPumpActive).Returns(true);


            task.UpdateStatus();
            
            _ioService.Verify();
            //no update because pump is already running
            _deviceService.Verify(x => x.SendReportedProperties(), Times.Never);
            Assert.Null(_deviceService.Object.Error);

        }

        [Fact]
        public void DisablePoolPumpTest()
        {

            var task = new TimerTask(_deviceService.Object, _ioService.Object, _timeService.Object);
            var config = new PoolPumpConfig
            {
                startTime = "13:00",
                stopTime = "14:00",
                enabled = true
            };
            var chlorineConfig = new ChlorinePumpConfig()
            {
                runId = 0,
                runtime = 0,
                enabled = true
            };

            _deviceService.Setup(x => x.PoolPumpConfig).Returns(config);
            _deviceService.Setup(x => x.ChlorinePumpConfig).Returns(chlorineConfig);
            _timeService.Setup(x => x.Now).Returns(new DateTime(2020, 1, 1, 14, 1, 0));
            _ioService.SetupGet(x => x.PoolPumpActive).Returns(true);


            task.UpdateStatus();

            _ioService.Verify();
            //no update because pump is already running
            _deviceService.Verify(x => x.SendReportedProperties(), Times.Once);
            _ioService.Verify(x => x.ChangePoolPumpStatus(false));
            Assert.Equal(false, _poolPumpStatus.active);
            Assert.Null(_deviceService.Object.Error);

        }


        [Theory]
        [InlineData("aaa","bbb")]
        [InlineData("13", "13")]
        [InlineData("a", "13:00")]
        [InlineData("13:00", "a")]
        [InlineData("", "")]
        public void InvalidPoolPumpTimeTest(string start, string end)
        {
            var task = new TimerTask(_deviceService.Object, _ioService.Object, _timeService.Object);
            var config = new PoolPumpConfig
            {
                startTime = start,
                stopTime = end,
                enabled = true
            };
            var chlorineConfig = new ChlorinePumpConfig()
            {
                runId = 0,
                runtime = 0,
                enabled = true
            };

            _deviceService.Setup(x => x.PoolPumpConfig).Returns(config);
            _deviceService.Setup(x => x.ChlorinePumpConfig).Returns(chlorineConfig);
            _timeService.Setup(x => x.Now).Returns(new DateTime(2020, 1, 1, 13,0, 0));
            _deviceService.Setup(x => x.SendReportedProperties()).Verifiable();

            task.UpdateStatus();

            _deviceService.Verify();
            Assert.NotNull(_deviceService.Object.Error);
            
            

        }
    }
}