using SerialTester;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    class MainVM : BaseViewModel
    {
        #region Commands --------------------------------------------
        public RelayCommand cmdSend { get; private set; }
        async void doSend(object o)
        {
            //int chkSum = loremIpsum.ToCharArray().Sum(c => c);
            var data = Encoding.UTF8.GetBytes(loremIpsum);

            isSending = !isSending;
            sendBtnTxt = isSending ? "Stop sending" : "Send";

            //if (isSending)
            //{
            //    teensyInterface.open();
            //}
            //else
            //{
            //    teensyInterface.close();
            //}

            var sw = new Stopwatch();
            sw.Start();
            int i = 0;
            while (isSending)
            {
                await teensyInterface.SendAsync(data);
                totalSent = 25.0 / 1024.0 * (++i);
                sendSpeed = totalSent / sw.Elapsed.TotalSeconds;
                OnPropertyChanged("sendSpeed");
                OnPropertyChanged("totalSent");
            }
            sw.Stop();
        }
        bool isSending = false;

        public RelayCommand cmdBreak { get; private set; }
        void doBreak(object o)
        {
            teensyInterface.brk();
        }



        public RelayCommand cmdUploadFW { get; private set; }
        async void doUpload(object o)
        {
            if (!teensyInterface.isConnected)
            {
                uploadMsg = teensyInterface.TeensyID + "\n" + "Please connect a Teensy";
                return;
            }

            String firmware = "";
            switch(teensyInterface.board)
            {
                case TeensySharp.PJRC_Board.Teensy_40:
                    firmware = "SerialTest_T4.hex";
                    break;

                case TeensySharp.PJRC_Board.Teensy_36:
                    firmware = "SerialTest_T36.hex";
                    break;

            }
            
            uploadMsg =
                $"Firmware Upload\n" +
                $"--------------------------------------\n" +
                $"{teensyInterface.TeensyID}\nUploading {firmware}...\n";

            int result = await Task.Run(() => teensyInterface.upload(firmware));
            switch (result)
            {
                case 0:
                    uploadMsg += "Success\nReconnecting...";
                    break;
                case 2:
                    uploadMsg += "Error during upload\n";
                    break;

                default:
                    uploadMsg += "Could not communicate to Teensy\n";
                    break;
            }
            await Task.Delay(1000);
            teensyInterface.connect();

            uploadMsg += "\n" + teensyInterface.TeensyID;
        }
        #endregion

        #region properties ------------------------------------------
        public TeensyInterface teensyInterface { get; }
        public String loremIpsum { get; private set; }
        public Double sendSpeed { get; set; }
        public Double totalSent { get; set; }
        public String sendBtnTxt
        {
            get => _sendBtnTxt;
            set => SetProperty(ref _sendBtnTxt, value);
        }
        String _sendBtnTxt = "Send";
        public String uploadMsg
        {
            get => _uploadMsg;
            private set => SetProperty(ref _uploadMsg, value);
        }
        String _uploadMsg;
        #endregion

        #region Construction ----------------------------------------
        public MainVM()
        {
            cmdSend = new RelayCommand(doSend);
            cmdUploadFW = new RelayCommand(doUpload);
            cmdBreak = new RelayCommand(doBreak);

            loremIpsum = File.ReadAllText("lorem.txt") + "\0";
            teensyInterface = new TeensyInterface();
            uploadMsg = teensyInterface.TeensyID;
        }
        #endregion
    }
}

