﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Devices;
using Meadow.Hardware;

namespace Meadow.Foundation.Audio.Mp3
{
    public partial class Yx5300
    {
        ISerialPort serialPort;

        protected Yx5300(ISerialPort serialPort)
        {
            this.serialPort = serialPort;

            serialPort.Open();

            SendCommand(Commands.Reset);

            Thread.Sleep(100);

            SendCommand(Commands.SelectDevice, 0, 2);

            Thread.Sleep(500);
        }

        public Yx5300(IMeadowDevice device, SerialPortName serialPortName)
            : this(device.CreateSerialPort(
                serialPortName))
        { }

        public void Reset()
        {
            SendCommand(Commands.Reset);
        }

        public void Sleep()
        {
            SendCommand(Commands.Sleep);
        }

        public void WakeUp()
        {
            SendCommand(Commands.Wake);
        }

        public void SetVolume(byte volume)
        {
            if (volume > 30) { volume = 30; }
            SendCommand(Commands.SetVolume, 0, volume);
        }

        public void VolumeUp()
        {
            SendCommand(Commands.VolumeUp);
        }

        public void VolumeDown()
        {
            SendCommand(Commands.VolumeDown);
        }

        public async Task<byte> GetVolume()
        {
            return (await SendCommandAndReadResponse(Commands.GetVolume)).Item2;
        }

        public async Task<byte> GetIndexOfCurrentFile()
        {
            return (await SendCommandAndReadResponse(Commands.GetCurrentFile)).Item2;
        }

        public async Task<byte> GetNumberOfFolders()
        {
            return (await SendCommandAndReadResponse(Commands.GetNumberOfFolders)).Item2;
        }

        public async Task<byte> GetNumberOfTracksInFolder(byte folderIndex)
        {
            return (await SendCommandAndReadResponse(Commands.GetNumberOfTracksInFolder, 0, folderIndex)).Item2;
        }

        public async Task<PlayStatus> GetStatus()
        {
            return (PlayStatus)(await SendCommandAndReadResponse(Commands.GetStatus)).Item2;
        }

        public void Play()
        {
            SendCommand(Commands.Play);
        }

        public void Play(byte songIndex)
        {
            SendCommand(Commands.PlayIndex, 0, songIndex);
        }

        public void Next()
        {
            SendCommand(Commands.Next);
        }

        public void Previous()
        {
            SendCommand(Commands.Previous);
        }

        public void Pause()
        {
            SendCommand(Commands.Pause);
        }

        public void Stop()
        {
            SendCommand(Commands.Stop);
        }

        private async Task<Tuple<Responses, byte>> SendCommandAndReadResponse(Commands command, byte data1 = 0, byte data2 = 0)
        {
            SendCommand(command, data1, data2);

            var response = ReadResponse();

            var data = ParseResponse(response);

            if (data.Item1 == Responses.DataReceived)
            {
                switch (command)
                {
                    case Commands.GetCurrentFile:
                    case Commands.GetNumberOfFolders:
                    case Commands.GetNumberOfTracksInFolder:
                    case Commands.GetStatus:
                    case Commands.GetTotalTracks:
                    case Commands.GetVolume:
                        {
                            await Task.Delay(500);
                            response = ReadResponse();
                            if (response.Length > 0)
                            {
                                data = ParseResponse(response);
                            }

                        }
                        break;
                }
            }
            return data;
        }

        private void SendCommand(Commands command, byte data1 = 0, byte data2 = 0)
        {
            byte[] sendBuffer = new byte[8];

            Thread.Sleep(20);

            // Command Structure 0x7E 0xFF 0x06 CMD FBACK DAT1 DAT2 0xEF
            sendBuffer[0] = 0x7E;    // Start byte
            sendBuffer[1] = 0xFF;    // Version
            sendBuffer[2] = 0x06;    // Command length not including Start and End byte.
            sendBuffer[3] = (byte)command; // Command
            sendBuffer[4] = 0x01;    // Feedback 0x00 NO, 0x01 YES
            sendBuffer[5] = data1;    // DATA1 datah
            sendBuffer[6] = data2;    // DATA2 datal
            sendBuffer[7] = 0xEF;    // End byte

            serialPort.Write(sendBuffer);
        }

        private byte[] ReadResponse()
        {
            byte[] response = new byte[12];
            byte value;
            int index = 0;

            do
            {
                if (serialPort.BytesToRead == 0)
                {
                    Console.WriteLine("No data available");
                    Thread.Sleep(50);
                    break;
                }

                value = (byte)serialPort.ReadByte();

                if (value == 0x7E) //new response
                {
                    index = 0;
                }
                response[index++] = value;
            }
            while (value != 0xEF);

            return response;
        }

        Tuple<Responses, byte> ParseResponse(byte[] data)
        {
            return new Tuple<Responses, byte>((Responses)(data[3]), data[6]);
        }
    }
}