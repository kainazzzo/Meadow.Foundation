﻿using System;
using Meadow.Hardware;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Rotary;

namespace Meadow.Foundation.Sensors.Rotary
{
    /// <summary>
    /// Digital rotary encoder that uses two-bit Gray Code to encode rotation and has an integrated push button.
    /// </summary>
    public class RotaryEncoderWithButton : RotaryEncoder, IRotaryEncoderWithButton
    {
        /// <summary>
        /// Returns the PushButton that represents the integrated button.
        /// </summary>
        public PushButton Button { get; private set; }

        /// <summary>
        /// Returns the push button's state
        /// </summary>
        public bool State => Button.State;

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a �press�.
        /// </summary>
        public event EventHandler Clicked = delegate { };

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        public event EventHandler PressEnded = delegate { };

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        public event EventHandler PressStarted = delegate { };

        /// <summary>
        /// Instantiates a new RotaryEncoder on the specified pins that has an integrated button.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="aPhasePin"></param>
        /// <param name="bPhasePin"></param>
        /// <param name="buttonPin"></param>
        /// <param name="buttonResistorMode"></param>
        public RotaryEncoderWithButton(IDigitalInputController device, IPin aPhasePin, IPin bPhasePin, IPin buttonPin, ResistorMode buttonResistorMode = ResistorMode.InternalPullDown)
            : base(device, aPhasePin, bPhasePin)
        {
            Button = new PushButton(device, buttonPin, buttonResistorMode);

            Button.Clicked += ButtonClicked;
            Button.PressEnded += ButtonPressEnded;
            Button.PressStarted += ButtonPressStarted;
        }

        protected void ButtonClicked(object sender, EventArgs e)
        {
            Clicked(this, e);
        }

        protected void ButtonPressEnded(object sender, EventArgs e)
        {
            PressEnded(this, e);
        }

        protected void ButtonPressStarted(object sender, EventArgs e)
        {
            PressStarted(this, e);
        }
    }
}