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
        private readonly IButton left;
        private readonly IButton up;
        private readonly IButton right;
        private readonly IButton down;
        private readonly bool isInverted;
        private DigitalJoystickPosition currentPosition = DigitalJoystickPosition.Center;



        private readonly object lockobj = new object();
        

        public DigitalJoystick(IButton up, IButton down, IButton left, IButton right, bool isInverted)
        {
            this.left = left;
            this.up = up;
            this.right = right;
            this.down = down;

            this.isInverted = isInverted;

            this.left.PressStarted += HandleButtons;
            this.up.PressStarted += HandleButtons;
            this.right.PressStarted += HandleButtons;
            this.down.PressStarted += HandleButtons;

            this.left.PressEnded += HandleButtons;
            this.up.PressEnded += HandleButtons;
            this.right.PressEnded += HandleButtons;
            this.down.PressEnded += HandleButtons;
        }

        public DigitalJoystick(IDigitalInputController device, IPin up, IPin down, IPin left, IPin right, bool isInverted = false)
: this(new PushButton(device, up), new PushButton(device, down), new PushButton(device, left), new PushButton(device, right), isInverted)
        {
        }


        private void HandleButtons(object sender, EventArgs e)
        {
            bool leftPressed = !left.State;
            bool upPressed = !up.State;
            bool rightPressed = !right.State;
            bool downPressed = !down.State;
            DigitalJoystickPosition oldPosition;

            lock (lockobj) {
                oldPosition = currentPosition;

                // It doesn't matter which button is pressed...
                // just update the position based on current values
                if (!leftPressed && !upPressed && !rightPressed && !downPressed)
                {
                    currentPosition = DigitalJoystickPosition.Center;
                }
                else if (upPressed && rightPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.DownRight : DigitalJoystickPosition.UpRight;
                }
                else if (downPressed && rightPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.UpRight : DigitalJoystickPosition.DownRight;
                }
                else if (downPressed && leftPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.UpLeft : DigitalJoystickPosition.DownLeft;
                }
                else if (upPressed && leftPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.DownLeft : DigitalJoystickPosition.UpLeft;
                }
                else if (leftPressed)
                {
                    currentPosition = DigitalJoystickPosition.Left;
                }
                else if (upPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.Down : DigitalJoystickPosition.Up;
                }
                else if (rightPressed)
                {
                    currentPosition = DigitalJoystickPosition.Right;
                }
                else if (downPressed)
                {
                    currentPosition = isInverted ? DigitalJoystickPosition.Up : DigitalJoystickPosition.Down;
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
