﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.BME280_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        Bme280 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            //CreateSpiSensor();
            CreateI2CSensor();

            var consumer = Bme280.CreateObserver(
                handler: result => 
                {
                    Console.WriteLine($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },                
                filter: result => 
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
                    if (result.Old is { } old) 
                    { 
                        return (
                        (result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5
                        &&
                        (result.New.Humidity.Value - old.Humidity.Value).Percent > 0.05
                        );
                    }
                    return false;
                }
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (sender, result) => {
                Console.WriteLine($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
                Console.WriteLine($"  Relative Humidity: {result.New.Humidity:N2}%");
                Console.WriteLine($"  Pressure: {result.New.Pressure?.Millibar:N2}mbar ({result.New.Pressure?.Pascal:N2}Pa)");
            };

            ReadConditions().Wait();

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        void CreateSpiSensor()
        {
            Console.WriteLine("Create BME280 sensor with SPI...");

            var spi = Device.CreateSpiBus();
            sensor = new Bme280(spi, Device.CreateDigitalOutputPort(Device.Pins.D00));
        }

        void CreateI2CSensor()
        {
            Console.WriteLine("Create BME280 sensor with I2C...");

            var i2c = Device.CreateI2cBus();
            sensor = new Bme280(i2c, (byte)Bme280.Addresses.Default); // SDA pulled up
            
        }

        async Task ReadConditions()
        {
            var conditions = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {conditions.Temperature?.Celsius:N2}C");
            Console.WriteLine($"  Pressure: {conditions.Pressure?.Bar:N2}hPa");
            Console.WriteLine($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%");
        }

        //<!—SNOP—>
    }
}