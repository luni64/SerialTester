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
        static byte[] data = (new byte[1024 * 25]).Select((b, idx) => (byte)idx).ToArray(); // generates 25kB byte array filled with increasing numbers 0,1,...25kByte

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                doReceive(args[0]);
            }
            else help();           
        }
        
        //-------------------------------------------------------------
        static void doReceive(String portname)
        {
            //int checksum  = data.Sum(d => d);
            //int lastByte = data[data.Length - 1];
            //Console.WriteLine($"last byte:{lastByte}, checksum: {checksum}");
                        
            Console.Clear();
            Console.CursorVisible = false;
            Console.SetCursorPosition(0,  5);
            Console.WriteLine("Any key to stop sending");

            try
            {
                port = new SerialPort(portname);
                port.Open();
                
                double sent = 0;
                var timer = new Stopwatch();
                timer.Start();

                while (!Console.KeyAvailable) // this while block will only send out multiples of 25kB
                {
                    // send out one data block (25kB)
                    port.Write(data, 0, data.Length);           

                    // calculate statistics
                    sent += data.Length;                        
                    double t = timer.Elapsed.TotalSeconds;
                    
                    // display results
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine($"Sent: {sent/1024/1024,8:F2} MByte");
                    Console.WriteLine($"Time: {t,8:F2} s");
                    Console.WriteLine($"Rate: {sent / t/1024/1024,8:F2} MByte/s");
                }

                Console.SetCursorPosition(0, 6);
                
                Console.WriteLine("Sending stopped, any key to close port and exit");
                Console.ReadKey(false);
                while (!Console.KeyAvailable) ;
                                
                port.Close();
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                port.Close();
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
