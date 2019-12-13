using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace SerialSender
{
    class Program
    {
        static SerialPort port;
        //static byte[] data = (new byte[1024 * 25]).Select((b, idx) => (byte)idx).ToArray(); // generates 25kB byte array filled with increasing numbers 0,1,...25kByte

        //-------------------------------------------------------------
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                //int checksum  = data.Sum(d => d);
                //int lastByte = data[data.Length - 1];
                //Console.WriteLine($"last byte:{lastByte}, checksum: {checksum}");

                byte[] data = prepareData(1024 * 25);
                
                              
                bool sending = true;

                Console.Clear();
                Console.CursorVisible = false;
                Console.SetCursorPosition(0, 5);
                Console.WriteLine("Space to toggle sending, ESC to quit");

                try
                {
                    port = new SerialPort(args[0]);  // port name passed as argument 0                    
                    port.Open();

                    int sentBlocks = 0;
                    var timer = new Stopwatch();
                    timer.Start();

                    while (true)
                    {
                        while (sending && !Console.KeyAvailable) // this while block will always send out multiples of 25kB
                        {
                            // send out one data block (25kB)
                            port.Write(data, 0, data.Length);

                            // calculate statistics

                            sentBlocks++;
                            double sentMB = sentBlocks * 25.0 / 1024.0; // kB;
                            double t = timer.Elapsed.TotalSeconds;

                            // display results
                            Console.SetCursorPosition(0, 0);
                            Console.WriteLine($"Blocks (25kB):{sentBlocks,8}");
                            Console.WriteLine($"Sent:         {sentMB,8:F2} MByte");
                            Console.WriteLine($"Time:         {t,8:F2} s");
                            Console.WriteLine($"Rate:         {sentMB / t,8:F2} MByte/s");
                            showResponse();
                        }

                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true).Key;
                            switch (key)
                            {
                                case ConsoleKey.Spacebar:
                                    sending = !sending;
                                    break;

                                case ConsoleKey.Escape:
                                    port.Close();
                                    Environment.Exit(0);
                                    break;
                            }
                        }

                        showResponse();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    port?.Close();
                }
            }
            else help();
        }

        //-------------------------------------------------------------

        static byte[] prepareData(int size)
        {
            byte[] data = new byte[size];

            // fill with random data
            var rnd = new Random();
            rnd.NextBytes(data);

            // Terminate with 0
            data[data.Length - 1] = 0; 

            // place checksum in first slot
            byte checkSum = (byte)data.Skip(1).Sum(d => d);
            data[0] = checkSum;

            return data;
        }

      
        static int errCnt = 0; 

        static void showResponse()
        {
            
            if (port.BytesToRead > 0)
            {
                String response = port.ReadLine();
                //int blockSize = int.Parse(response);

                Console.SetCursorPosition(0, 8);
                //Console.WriteLine($"#: {errCnt} wrong block size:{response}, expected: {1024*25}, difference: {1024*25-blockSize}");
                Console.WriteLine(response);

                Task.Delay(200).Wait();
            }
        }

        static void help()
        {
            Console.WriteLine("usage: SerialSender COMx (replace x by port number)\n");
            Console.WriteLine("Any key to quit");
            while (!Console.KeyAvailable) ;
            Environment.Exit(1);
        }
    }
}
