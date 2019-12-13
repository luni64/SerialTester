using HidLibrary;
using System;


namespace lunOptics.TeensySharp
{
    public enum PJRC_Board
    {
        Teensy_40,
        Teensy_36,
        Teensy_35,
        Teensy_31_2,
        Teensy_30,
        Teensy_LC,
        Teensy_2pp,
        Teensy_2,
        unknown,
    }

    public enum UsbType
    {
        UsbSerial,
        HalfKay,
        HID,
    }

    public interface ITeensy
    {
        UsbType UsbType { get; }
        uint Serialnumber { get; }
        String Port { get; }
        PJRC_Board BoardType { get; }
        String BoardId { get; }
               
        bool Reboot();
        bool Upload(IFirmware firmware, bool checkType = true, bool reset = true);
        bool Reset();

      //  HidDevice hidDevice { get; }
    }
}
