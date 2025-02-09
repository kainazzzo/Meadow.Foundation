using Meadow.Devices;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    //aka WaveShare EPD2i13
    /// <summary>
    ///     Represents an Il3897 ePaper display
    /// </summary>
    public class Il3897 : EpdBase
    {
        public Il3897(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin,
            int width = 122, int height = 250) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, width, height)
        { }

        protected override void Initialize()
        {
            Reset();

            SendCommand(Command.DRIVER_OUTPUT_CONTROL);
            SendData((int)(Width - 1));
            SendData((int)(Width - 1) >> 8);
            SendData(0x00);                     // GD = 0; SM = 0; TB = 0;

            SendCommand(Command.BOOSTER_SOFT_START_CONTROL);
            SendData(0xD7);
            SendData(0xD6);
            SendData(0x9D);

            SendCommand(Command.WRITE_VCOM_REGISTER);
            SendData(0xA8);                     // VCOM 7C

            SendCommand(Command.SET_DUMMY_LINE_PERIOD);
            SendData(0x1A);                     // 4 dummy lines per gate

            SendCommand(Command.SET_GATE_TIME);
            SendData(0x08);                     // 2us per line

            SendCommand(Command.DATA_ENTRY_MODE_SETTING);
            SendData(0x03);                     // X increment; Y increment

            SendData(LUT_Full_Update);
        }

        public static readonly byte[] LUT_Full_Update =
        {
            0x22, 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x11,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E, 0x1E,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        public static readonly byte[] LUT_Partial_Update =
        {
            0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0F, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
    }
}
