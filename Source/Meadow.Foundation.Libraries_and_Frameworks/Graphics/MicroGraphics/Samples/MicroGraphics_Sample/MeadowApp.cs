using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;

namespace µGraphics_Basic_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        // internals
        int dW = 240; // display width 
        int dH = 240; // display height
        //int dH = 320;
        protected St7789 display;
        //protected Ili9341 display;
        protected GraphicsLibrary canvas;

        RgbPwmLed onboardLed;

        Color[] Colors = new Color[] {
                WildernessLabsColors.AzureBlue,
                WildernessLabsColors.PearGreen,
                WildernessLabsColors.ChileanFire,
                WildernessLabsColors.GalleryWhite };

        public MeadowApp()
        {
            Initialize();

            while (true)
            {


                this.DrawCoordinateAxes();
                Thread.Sleep(2000);
                this.FillDisplayColors();


                this.DrawFastCardinalLines();
                //Thread.Sleep(2000);
                //this.FillWithRandomColors();
                //Thread.Sleep(2000);
                this.StrokeTest();
                //Thread.Sleep(2000);

                this.DrawRoundedRectangles();
                //Thread.Sleep(2000);
                this.DrawPolarLines();
                Thread.Sleep(2000);
                //this.DrawFunFilledRectangles();
                //Thread.Sleep(2000);
                //this.DrawText();
                //Thread.Sleep(2000);
                //this.FillDisplayColors();
                //Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// intializes the physical display peripheral, as well as the backing
        /// canvas/graphics utility library.
        /// </summary>
        protected void Initialize()
        {
            Console.WriteLine("Initializing hardware...");

            onboardLed = new RgbPwmLed(
                Device,
                Device.Pins.OnboardLedRed,
                Device.Pins.OnboardLedGreen,
                Device.Pins.OnboardLedBlue,
                commonType: IRgbLed.CommonType.CommonAnode);

            // new up the actual display on the SPI bus
            // our display needs mode3
            var config = new SpiClockConfiguration(48000, SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);
            display = new St7789
            (
                device: MeadowApp.Device,
                spiBus: spiBus,
                chipSelectPin: MeadowApp.Device.Pins.D02,
                dcPin: MeadowApp.Device.Pins.D01,
                resetPin: MeadowApp.Device.Pins.D00,
                width: (uint)dW, height: (uint)dH
                //displayColorMode: St7789.DisplayColorMode.Format16bppRgb565
            );

            //var config = new SpiClockConfiguration(48000);
            //display = new Ili9341(
            //    device: Device,
            //    spiBus: MeadowApp.Device.CreateSpiBus(MeadowApp.Device.Pins.SCK, MeadowApp.Device.Pins.MOSI, MeadowApp.Device.Pins.MISO, config),
            //    chipSelectPin: Device.Pins.A03,
            //    dcPin: null,//Device.Pins.A05,
            //    resetPin: Device.Pins.A04,
            //    width: 240, height: 320
            //    );

            // The graphics library/canvas needs to be initialized with the display,
            // becuase the display driver itself is responsible creating the
            // appropriate display buffer (which is specific to each display
            // because each display has its own size and color struture) and
            // knows how to actualy push the buffer to the device and whatnot.
            // The graphics library itself only knows how to draw to the buffer.
            // Also important to note that different displays represent pixels
            // with varying size byte structures, depending on the color-depth of
            // the display.
            canvas = new GraphicsLibrary(display)
            {
                // Depending on how your display is rotated, this is a good place
                // to set the default orientation
                Rotation = GraphicsLibrary.RotationType._180Degrees,
            };

            // sometimes, when the display boots, it'll have artifacts present,
            // this cleans the canvas and passing `true` will copy the canvas to the
            // display immediately
            Console.WriteLine("Clearing display");
            canvas.Clear(true);
        }

        /// <summary>
        /// Fill the display with various colors.
        /// </summary>
        public void FillDisplayColors()
        {
            Console.WriteLine("FillDisplayColors()");

            // these don't seem to do anything
            //display.ClearScreen(8);
            //display.ClearScreen(156);
            //display.ClearScreen(1000);
            //display.ClearScreen(40980);

            foreach (var color in Colors)
            {
                FillWithColor(color);
            }
        }

        public void DrawFunFilledRectangles()
        {
            canvas.Clear();
            DrawColorfulRectangle(10, 10, 100, 50);
            DrawColorfulRectangle(100, 100, 50, 100);
            canvas.Show();
        }

        void DrawText()
        {
            // clear our buffer
            canvas.Clear();

            canvas.CurrentFont = new Font12x20();
            canvas.DrawText(0, 0, "2x Scale", WildernessLabsColors.AzureBlue, GraphicsLibrary.ScaleFactor.X2);
            canvas.DrawText(0, 48, "12x20 Font", WildernessLabsColors.PearGreen, GraphicsLibrary.ScaleFactor.X2);
            canvas.DrawText(0, 96, "0123456789", WildernessLabsColors.GalleryWhite, GraphicsLibrary.ScaleFactor.X2);
            canvas.DrawText(0, 144, "!@#$%^&*()", WildernessLabsColors.ChileanFire, GraphicsLibrary.ScaleFactor.X2);
            canvas.DrawText(0, 192, "3x!", WildernessLabsColors.DustyGray, GraphicsLibrary.ScaleFactor.X3);

            canvas.Show();
        }

        /// <summary>
        /// Horizontal and vertical lines can be drawn via `DrawHorizontalLine`
        /// and `DrawVerticalLine` slightly faster than if calling `DrawLine`.
        /// </summary>
        void DrawFastCardinalLines()
        {
            // clear our buffer
            canvas.Clear();

            // set our pen width
            canvas.Stroke = 1;

            int colorIndex = 0;
            for (int i = 0; i + canvas.Stroke + 2 < dH; i += canvas.Stroke + 2)
            {

                // HACK: fast line drawing does't seem to respect stroke, so we
                // have to draw multiple lines
                for (int j = 0; j < canvas.Stroke; j++)
                {
                    canvas.DrawHorizontalLine(10, i + j, dW - 10, Colors[colorIndex]);
                }

                // increment our color index to go through all colors
                if (colorIndex < Colors.Length - 1)
                {
                    colorIndex++;
                }
                else { colorIndex = 0; }
                canvas.Stroke++;
            }
            // send our art to the gallery
            canvas.Show();

            // reset our pen width
            canvas.Stroke = 1;
        }


        // drawing helpers
        void FillWithColor(Color color)
        {
            // clear our buffer
            canvas.Clear();
            // draw a filled rectangle that fills the screen
            canvas.DrawRectangle(0, 0, dW, dH, color, filled: true);
            // copy the canvas contents to the device
            onboardLed.SetColor(color);
            canvas.Show();
        }

        //void FillWithRandomColors()
        //{
        //    try {
        //        canvas.Clear();
        //        int x, y;
        //        Console.WriteLine("here1");
        //        for (int i = 0; i < dH * dW; i++) {
        //            x = (i % dW);
        //            y = (i % dH);
        //            Console.WriteLine($"x:{x},y:{y}");
        //            canvas.DrawPixel(x, y, GenerateRandomColor());
        //        }
        //        canvas.Show();
        //    } catch (Exception e) {
        //        Console.WriteLine(e.Message);
        //    }

        //}

        /// <summary>
        /// Draws some 
        /// </summary>
        void DrawPolarLines()
        {
            canvas.Clear();
            canvas.Stroke = 1;

            int colorIndex = 0;
            for (int i = 0; i < 270; i += 12)
            {

                canvas.DrawLine(dW / 2, dH / 2, (dW <= dH ? dW / 2 - 10 : dH / 2 - 10),
                    (float)(i * Math.PI / 180), Colors[colorIndex]);
                canvas.Stroke++;

                // increment our color index to go through all colors
                if (colorIndex < Colors.Length - 1)
                {
                    colorIndex++;
                }
                else { colorIndex = 0; }
                canvas.Stroke++;
            }

            canvas.Show();
        }

        void DrawRoundedRectangles()
        {
            // clear the buffer
            canvas.Clear();

            canvas.Stroke = 1;

            // draw five random rounded rectangles
            Color currentColor;
            for (int i = 0; i < 5; i++)
            {
                Random rnd = new Random(DateTime.Now.Millisecond);
                currentColor = (i < Colors.Length ? Colors[i] : Colors[i % Colors.Length]);
                var width = rnd.Next(2, dW - 25);
                var height = rnd.Next(2, dH - 25);
                var x = rnd.Next(dW - width + 1);
                var y = rnd.Next(dH - height + 1);
                var radius = rnd.Next(1, ((width < height) ? width / 2 : height / 2) + 1);
                var filled = rnd.Next(0, 2) == 0 ? false : true;
                canvas.DrawRoundedRectangle(x, y, width, height, radius, currentColor, filled);
            }

            canvas.Show();
        }

        void StrokeTest()
        {
            canvas.Clear();

            canvas.Stroke = 1;
            canvas.DrawLine(5, 5, 115, 5, Color.SteelBlue);
            canvas.Stroke = 2;
            canvas.DrawLine(5, 25, 115, 25, Color.SteelBlue);
            canvas.Stroke = 3;
            canvas.DrawLine(5, 45, 115, 45, Color.SteelBlue);
            canvas.Stroke = 4;
            canvas.DrawLine(5, 65, 115, 65, Color.SteelBlue);
            canvas.Stroke = 5;
            canvas.DrawLine(5, 85, 115, 85, Color.SteelBlue);

            canvas.Stroke = 1;
            canvas.DrawLine(135, 5, 135, 115, Color.SlateGray);
            canvas.Stroke = 2;
            canvas.DrawLine(155, 5, 155, 115, Color.SlateGray);
            canvas.Stroke = 3;
            canvas.DrawLine(175, 5, 175, 115, Color.SlateGray);
            canvas.Stroke = 4;
            canvas.DrawLine(195, 5, 195, 115, Color.SlateGray);
            canvas.Stroke = 5;
            canvas.DrawLine(215, 5, 215, 115, Color.SlateGray);

            canvas.Stroke = 1;
            canvas.DrawLine(5, 125, 115, 235, Color.Silver);
            canvas.Stroke = 2;
            canvas.DrawLine(25, 125, 135, 235, Color.Silver);
            canvas.Stroke = 3;
            canvas.DrawLine(45, 125, 155, 235, Color.Silver);
            canvas.Stroke = 4;
            canvas.DrawLine(65, 125, 175, 235, Color.Silver);
            canvas.Stroke = 5;
            canvas.DrawLine(85, 125, 195, 235, Color.Silver);

            canvas.Stroke = 2;
            canvas.DrawRectangle(2, 2, 236, 236, Color.DimGray, false);

            canvas.Show();
        }

        void DrawColorfulRectangle(int x, int y, int width, int height)
        {

            int shells = (width < height ? width : height);
            for (int i = 0; i < shells; i++)
            {
                Color currentColor = (i < Colors.Length ? Colors[i] : Colors[i % Colors.Length]);
                canvas.DrawRectangle(x + i, y + i, width - (i * 2), height - (i * 2), currentColor);
            }
        }

        Color GenerateRandomColor()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            return Color.FromRgb(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256));
        }

        void DrawCoordinateAxes()
        {
            canvas.Clear();

            canvas.CurrentFont = new Font8x12();
            // Y-Axis
            int y = dH - 1;
            int numberOfVerticalTicks = (dH / 20) - 1;
            for (int i = numberOfVerticalTicks; i >= 0; i--)
            {
                canvas.DrawHorizontalLine(0, y, 10, WildernessLabsColors.AzureBlue);
                if (i > 0)
                { // don't draw `0`
                    // draw text every 20 ticks
                    if (i % 2 == 1)
                    {
                        canvas.DrawText(12, y - canvas.CurrentFont.Height / 2, (y + 1).ToString(), WildernessLabsColors.ChileanFire);
                    }
                }
                y -= 20;
            }
            // X-Axis
            int tickInterval = 20;
            int x = tickInterval; // start at the first, not 0
            int numberOfHorizontalTicks = (dW / tickInterval) - 1;
            for (int i = 0; i < numberOfHorizontalTicks; i++)
            {
                canvas.DrawVerticalLine(x, 10, 10, WildernessLabsColors.GalleryWhite);
                // draw text every other tick
                if (i % 2 == 1)
                {
                    canvas.DrawText(x, 12 + canvas.CurrentFont.Height, x.ToString(), WildernessLabsColors.ChileanFire, alignment: GraphicsLibrary.TextAlignment.Center);
                }
                x += 20;
            }

            canvas.Show();
            ////---- (250,700)
            //context.FillEllipseInRect(new RectangleF(250, 700, 6, 6));
            //this.ShowCenteredTextAtPoint(context, 250, 695, "(250,700)", textHeight);

            ////---- (500,300)
            //context.FillEllipseInRect(new RectangleF(500, 300, 6, 6));
            //this.ShowCenteredTextAtPoint(context, 500, 295, "(500,300)", textHeight);

        }

    }
}