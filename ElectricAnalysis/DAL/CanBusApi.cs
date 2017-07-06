using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.DAL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CAN_MSG_T
    {
        public byte rtr;
        public uint id;
        public byte dlen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] data;
    }
    class CanBusApi
    {
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANSelectDevice",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANSelectDevice(IntPtr hwnd, int GetModule, ref uint DeviceNum, byte[] Description);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANPortOpen",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANPortOpen(UInt16 DevNum, ref UInt16 wPort, ref UInt16 wHostID, ref UInt16 wBaudRate);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANPortClose",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANPortClose(UInt16 wPort);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANInit",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANInit(UInt16 wPort, UInt16 BTR0, UInt16 BTR1, byte ucMask);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANSetBaud",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANSetBaud(UInt16 wPort, UInt16 BTR0, UInt16 BTR1);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANSetAcp",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANSetAcp(UInt16 wPort, UInt16 Acp, UInt16 Mask);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANSetOutCtrl",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANSetOutCtrl(UInt16 wPort, UInt16 OutCtrl);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANSetNormal",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANSetNormal(UInt16 wPort);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANHwReset",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANHwReset(UInt16 wPort);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANEnableEvent",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANEnableEvent(UInt16 wPort, int Enabled);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANEnableRxInt",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANEnableRxInt(UInt16 wPort);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANReadFile",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANReadFile(UInt16 wPort, Int32 NumNeeded, ref CAN_MSG_T RcvBuf, ref Int32 pNumReturned);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANWriteFile",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANWriteFile(UInt16 wPort, ref int Ready, ref CAN_MSG_T SendBuf);
        [DllImport("AdsCAN.dll",
            CallingConvention = CallingConvention.StdCall,
            EntryPoint = "CANSetProtocolType",
            CharSet = CharSet.Unicode)]
        public extern static Int32 CANSetProtocolType(UInt16 wPort, UInt16 wProtocolType);
    }
}
