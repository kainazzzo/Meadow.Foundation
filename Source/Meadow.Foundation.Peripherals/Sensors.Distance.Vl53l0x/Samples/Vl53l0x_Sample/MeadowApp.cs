﻿using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using Meadow.Units;
using LU = Meadow.Units.Length.UnitType;

namespace Sensors.Distance.Vl53l0x_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        Vl53l0x sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing hardware...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(Device, i2cBus, (byte)Vl53l0x.Addresses.Default);

            sensor.DistanceUpdated += Sensor_Updated;
            sensor.StartUpdating(TimeSpan.FromMilliseconds(250));
        }

        private void Sensor_Updated(object sender, IChangeResult<Length> result)
        {
            if (result.New == null) { return; }

            if (result.New < new Length(0, LU.Millimeters))
            { 
                Console.WriteLine("out of range.");
            }
            else 
            {
                Console.WriteLine($"{result.New.Millimeters}mm / {result.New.Inches:n3}\"");
            }
        }

        //<!—SNOP—>

        void InitializeWithShutdownPin()
        {
            Console.WriteLine("Initialize hardware...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(Device, i2cBus, Device.Pins.D05, 250);
        }
    }
}