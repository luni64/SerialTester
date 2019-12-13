using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
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

        public void Send(byte[] data)
        {
            CancellationTokenSource ts = new CancellationTokenSource();
            var ct = ts.Token;

            try
            {
                port.WriteTimeout = 1000;
                port.ReadTimeout = 1000;
                port.Handshake = Handshake.RequestToSend;


                ts.CancelAfter(1000);

                port.BaseStream.WriteAsync(data, 0, data.Length, ct);
            }
            catch (Exception e)
            {
                var x = e.GetType();
            }
        }

        public void brk()
        {
            ts.Cancel();
        }

        CancellationTokenSource ts = new CancellationTokenSource(5000);

        async public Task SendAsync(byte[] data)
        {
            var ct = ts.Token;

            try
            {
                // ct.ThrowIfCancellationRequested();
                //port.WriteTimeout = 1000;
                //port.ReadTimeout = 1000;
                //port.Handshake = Handshake.RequestToSend;

                var s = port.BaseStream;
               

                await port.BaseStream.WriteAsync(data, 0, data.Length, ct);
            }
            catch (Exception e)
            {
                var x = e.GetType();
            }



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

        public void open() { port.Open(); }
        public void close()
        {
            try
            {
                Thread CloseDown = new Thread(new ThreadStart(() => port.Close()));
            }
            catch { }
        }

        public bool connect()
        {
            var watcher = new TeensyWatcher();
            teensy = watcher.ConnectedDevices
                .FirstOrDefault();

            if (teensy != null && !String.IsNullOrEmpty(teensy.Port))
            {
                try
                {
                    port = new SerialPort(teensy.Port);
                    port.WriteBufferSize = 1024 * 30;
                    // port.WriteTimeout = 1000;
                    port.Open();
                    //port.Close();
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

