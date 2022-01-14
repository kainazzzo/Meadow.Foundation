using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Hid;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Hid
{
    public class DigitalJoystick : SensorBase<DigitalJoystickPosition>
    {
        private readonly IButton topLeft;
        private readonly IButton topRight;
        private readonly IButton bottomRight;
        private readonly IButton bottomLeft;
        private readonly bool isInverted;
        private DigitalJoystickPosition currentPosition = DigitalJoystickPosition.Center;



        private readonly object lockobj = new object();
        

        public DigitalJoystick(IButton topLeft, IButton topRight, IButton bottomRight, IButton bottomLeft, bool isInverted)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;
            this.isInverted = isInverted;

            this.topLeft.PressStarted += HandleButtons;
            this.topRight.PressStarted += HandleButtons;
            this.bottomRight.PressStarted += HandleButtons;
            this.bottomLeft.PressStarted += HandleButtons;

            this.topLeft.PressEnded += HandleButtons;
            this.topRight.PressEnded += HandleButtons;
            this.bottomRight.PressEnded += HandleButtons;
            this.bottomLeft.PressEnded += HandleButtons;
        }

        public DigitalJoystick(IDigitalInputController device, IPin topLeft, IPin topRight, IPin bottomRight, IPin bottomLeft, bool isInverted = false)
: this(new PushButton(device, topLeft), new PushButton(device, topRight), new PushButton(device, bottomRight), new PushButton(device, bottomLeft), isInverted)
        {
        }


        private void HandleButtons(object sender, EventArgs e)
        {
            bool topLeftPressed = !topLeft.State;
            bool topRightPressed = !topRight.State;
            bool bottomRightPressed = !bottomRight.State;
            bool bottomLeftPressed = !bottomLeft.State;
            DigitalJoystickPosition oldPosition;

            lock (lockobj) {
                oldPosition = currentPosition;

                // It doesn't matter which button is pressed...
                // just update the position based on current values
                if (!topLeftPressed && !topRightPressed && !bottomRightPressed && !bottomLeftPressed)
                {
                    currentPosition = DigitalJoystickPosition.Center;
                }
                else if (topLeftPressed && topRightPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.Down : DigitalJoystickPosition.Up;
                }
                else if (topRightPressed && bottomRightPressed)
                {
                    currentPosition = DigitalJoystickPosition.Right;
                }
                else if (bottomRightPressed && bottomLeftPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.Up : DigitalJoystickPosition.Down;
                }
                else if (bottomLeftPressed && topLeftPressed)
                {
                    currentPosition = DigitalJoystickPosition.Left;
                }
                else if (topLeftPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.DownLeft : DigitalJoystickPosition.UpLeft;
                }
                else if (topRightPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.DownRight : DigitalJoystickPosition.UpRight;
                }
                else if (bottomRightPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.UpRight : DigitalJoystickPosition.DownRight;
                }
                else if (bottomLeftPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.UpLeft : DigitalJoystickPosition.DownLeft;
                }

                var result = new ChangeResult<DigitalJoystickPosition>(currentPosition, oldPosition);
                base.RaiseEventsAndNotify(result);
            }
        }




        protected override Task<DigitalJoystickPosition> ReadSensor()
        {
            // This class uses events, so technically it's not reading now.
            return Task.FromResult(currentPosition);
        }
    }
}
