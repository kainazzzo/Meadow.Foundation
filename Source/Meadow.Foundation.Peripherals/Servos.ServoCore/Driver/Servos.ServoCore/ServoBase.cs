﻿using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Servos
{
    public abstract class ServoBase : IServo
    {
        protected IPwmPort _pwm = null;

        /// <summary>
        /// Gets the ServoConfig that describes this servo.
        /// </summary>
        public ServoConfig Config
        {
            get { return _config; }
        } protected ServoConfig _config = null;

        /// <summary>
        /// Returns the current angle. Returns -1 if the angle is unknown.
        /// </summary>
        public Angle? Angle { get; protected set; }

        /// <summary>
        /// Instantiates a new Servo on the specified PWM Pin with the specified config.
        /// </summary>
        /// <param name="pwm"></param>
        /// <param name="config"></param>
        public ServoBase(IPwmPort pwm, ServoConfig config)
        {
            _config = config;

            _pwm = pwm;
            _pwm.Frequency = config.Frequency;
            _pwm.DutyCycle = 0;
            _pwm.Start();
        }

        /// <summary>
        /// Rotates the servo to a given angle.
        /// </summary>
        /// <param name="angle">The angle to rotate to.</param>
        public void RotateTo(Angle angle)
        {
            // angle check
            if (angle < _config.MinimumAngle || angle > _config.MaximumAngle) {
                throw new ArgumentOutOfRangeException(nameof(angle), "Angle must be within servo configuration tolerance.");
            }

            // calculate the appropriate pulse duration for the angle
            float pulseDuration = CalculatePulseDuration(angle);
            //Console.WriteLine("Pulse Duration: " + pulseDuration.ToString());

            // send our pulse to the servo to make it move
            SendCommandPulse(pulseDuration);

            // update the state
            Angle = angle;
        }

        /// <summary>
        /// Stops the signal that controls the servo angle. For many servos, this will 
        /// return the servo to its nuetral position (usually 0º).
        /// </summary>
        public void Stop()
        {
            _pwm.Stop();
            Angle = null;
        }

        protected float CalculatePulseDuration(Angle angle)
        {
            // offset + (angle percent * duration length)
            return _config.MinimumPulseDuration + (float)((angle.Degrees / _config.MaximumAngle.Degrees) * (_config.MaximumPulseDuration - _config.MinimumPulseDuration));
            // sample calcs:
            // 0 degrees time = 1000 + ( (0 / 180) * 1000 ) = 1,000 microseconds
            // 90 degrees time = 1000 + ( (90 / 180) * 1000 ) = 1,500 microseconds
            // 180 degrees time = 1000 + ( (180 / 180) * 1000 ) = 2,000 microseconds
        }

        /// <summary>
        /// Note that this calculation expects a pulse duration in _microseconds_.
        /// </summary>
        /// <param name="pulseDuration">Microseconds</param>
        /// <returns></returns>
        protected float CalculateDutyCycle(float pulseDuration)
        {
            // the pulse duration is dependent on the frequency we're driving the servo at
            return pulseDuration / ((1.0f / (float)_config.Frequency) * 1000000f);
        }

        protected void SendCommandPulse(float pulseDuration)
        {
            //Console.WriteLine($"Sending Command Pulse, duration {pulseDuration}, dutycycle: {CalculateDutyCycle(pulseDuration)}");
            _pwm.DutyCycle = CalculateDutyCycle(pulseDuration);
        }
    }
}