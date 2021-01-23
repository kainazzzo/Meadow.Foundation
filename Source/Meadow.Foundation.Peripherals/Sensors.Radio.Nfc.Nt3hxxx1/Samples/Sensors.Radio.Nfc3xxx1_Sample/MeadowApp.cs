using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Sensors.Radio.Nfc;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected Nt3hxxx1 nfc;
        protected II2cBus i2c;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            i2c = Device.CreateI2cBus(I2cBusSpeed.Standard);
            nfc = new Nt3hxxx1(i2c);
        }

    }
}