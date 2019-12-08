using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensySharp;

namespace SerialTester
{
    class TeensyInterface
    {
        public TeensyInterface()
        {
            connect();
        }
        public bool isConnected { get; private set; }
        public string TeensyID { get; private set; }

        async public Task Send(byte[] data)
        {
            await port.BaseStream.WriteAsync(data, 0, data.Length);
        }

        public int upload(string filename)
        {
            if (!isConnected) return -1;

            port.Close();

            var image = SharpUploader.GetEmptyFlashImage(teensy.BoardType);
            using (var file = new StreamReader(filename))
            {
                SharpHexParser.ParseStream(file, image);

            SharpUploader.StartHalfKay(serialnumber);
            return SharpUploader.Upload(image, teensy.BoardType, serialnumber);
            }
        }

        public uint serialnumber { get; private set; } = 0;
        public PJRC_Board board { get; private set; } = PJRC_Board.unknown;

        public bool connect()
        {
            var watcher = new TeensyWatcher();
            teensy = watcher.ConnectedDevices
                .FirstOrDefault(t => t.BoardType == PJRC_Board.Teensy_40 );

            if (teensy != null && !String.IsNullOrEmpty(teensy.Port))
            {
                try
                {
                    port = new SerialPort(teensy.Port);
                    port.Open();
                }
                catch
                {
                    port?.Dispose();
                    port = null;
                    TeensyID = $"Can't open port {teensy.BoardId}";
                    return false;
                }
                TeensyID = $"{teensy.BoardId} connected";
                serialnumber = teensy.Serialnumber;
                board = teensy.BoardType;
                isConnected = true;
                return true;
            }
            else
            {
                TeensyID = $"No compatible Teensy found";
                return false;
            }
        }

        private USB_Device teensy;

        private SerialPort port;
    }
}
