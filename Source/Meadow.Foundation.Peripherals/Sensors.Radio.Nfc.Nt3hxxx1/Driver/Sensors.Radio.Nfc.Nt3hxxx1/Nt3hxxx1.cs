using System;
using Meadow.Hardware;


namespace Meadow.Foundation.Sensors.Radio.Nfc
{
    public class Nt3hxxx1
    {
        protected II2cBus bus;
        protected const byte address = 0x55;

        private Nt3hxxx1() { }

        public Nt3hxxx1(II2cBus i2c)
        {
            bus = i2c;
            Init();
        }

        protected void Init()
        {
            bus.WriteData(address, 0x00);
            byte[] read = bus.ReadData(0x55, 16);
            foreach (byte b in read) {
                Console.Write($"{b:X},");
            }
        }
    }
}
