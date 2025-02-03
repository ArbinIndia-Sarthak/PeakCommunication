using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PeakCommunication
{
    public class PcanBasic
    {
        // Load the PCAN-Basic DLL
        private const string PcanBasicDll = "PCANBasic.dll";

        // PCAN-Basic Channels
        public const uint PCAN_USB = 0x51; // PCAN-USB Channel

        // Message Type
        public struct TPCANMsg
        {
            public uint ID;
            public byte LEN;
            public byte TYPE;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] DATA;
        }

        // Import PCAN-Basic functions
        [DllImport(PcanBasicDll, EntryPoint = "CAN_Initialize")]
        public static extern uint CAN_Initialize(uint Channel, ushort Btr0Btr1, uint HwType, uint IOPort, ushort Interrupt);

        [DllImport(PcanBasicDll, EntryPoint = "CAN_Write")]
        public static extern uint CAN_Write(uint Channel, ref TPCANMsg Message);

        [DllImport(PcanBasicDll, EntryPoint = "CAN_Read")]
        public static extern uint CAN_Read(uint Channel, out TPCANMsg Message, IntPtr Timestamp);

        [DllImport(PcanBasicDll, EntryPoint = "CAN_Uninitialize")]
        public static extern uint CAN_Uninitialize(uint Channel);
    }
}
