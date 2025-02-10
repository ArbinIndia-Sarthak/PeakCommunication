using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Peak.Can.Basic.BackwardCompatibility;
using Peak.Can.Basic;
using System.Threading.Channels;
using TPCANHandle = System.UInt16;
using TPCANBitrateFD = System.String;


namespace PeakCommunication
{
    public class PcanBasic
    {
        // Load the PCAN-Basic DLL
        private const string PcanBasicDll = "PCANBasic.dll";

        // PCAN-Basic Channels
        public const TPCANHandle PCAN_USB = (TPCANHandle)0x51;


        // Message Type
        public struct TPCANMsg
        {
            public uint ID;
            [MarshalAs(UnmanagedType.U1)]
            public TPCANMessageType MSGTYPE;
            public byte DLC;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] DATA;
        }
        public struct TPCANMsgFD
        {
            public uint ID;
            [MarshalAs(UnmanagedType.U1)]
            public TPCANMessageType MSGTYPE;
            public byte DLC;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] DATA;
        }


        [Flags]
        public enum TPCANMessageType : byte
        {
            PCAN_MESSAGE_STANDARD = 0x00,
            PCAN_MESSAGE_RTR = 0x01,
            PCAN_MESSAGE_EXTENDED = 0x02,
            PCAN_MESSAGE_FD = 0x04,
            PCAN_MESSAGE_BRS = 0x08,
            PCAN_MESSAGE_ESI = 0x10,
            PCAN_MESSAGE_ERRFRAME = 0x40,
            PCAN_MESSAGE_STATUS = 0x80,
        }
        public enum TPCANStatus : uint
        {
            PCAN_ERROR_OK = 0x00000,
            PCAN_ERROR_XMTFULL = 0x00001,
            PCAN_ERROR_OVERRUN = 0x00002,
            PCAN_ERROR_BUSLIGHT = 0x00004,
            PCAN_ERROR_BUSHEAVY = 0x00008,
            PCAN_ERROR_BUSWARNING = PCAN_ERROR_BUSHEAVY,
            PCAN_ERROR_BUSPASSIVE = 0x40000,
            PCAN_ERROR_BUSOFF = 0x00010,
            PCAN_ERROR_ANYBUSERR = (PCAN_ERROR_BUSWARNING),
            PCAN_ERROR_QRCVEMPTY = 0x00020,
            PCAN_ERROR_QOVERRUN = 0x00040,
            PCAN_ERROR_QXMTFULL = 0x00080,
            PCAN_ERROR_REGTEST = 0x00100,
            PCAN_ERROR_NODRIVER = 0x00200,
            PCAN_ERROR_HWINUSE = 0x00400,
            PCAN_ERROR_NETINUSE = 0x00800,
            PCAN_ERROR_ILLHW = 0x01400,
            PCAN_ERROR_ILLNET = 0x01800,
            PCAN_ERROR_ILLCLIENT = 0x01C00,
            PCAN_ERROR_ILLHANDLE = PCAN_ERROR_ILLHW,
            PCAN_ERROR_RESOURCE = 0x02000,
            PCAN_ERROR_ILLPARAMTYPE = 0x04000,
            PCAN_ERROR_ILLPARAMVAL = 0x08000,
            PCAN_ERROR_UNKNOWN = 0x10000,
            PCAN_ERROR_ILLDATA = 0x20000,
            PCAN_ERROR_CAUTION = 0x2000000,
            PCAN_ERROR_INITIALIZE = 0x4000000,
            PCAN_ERROR_ILLOPERATION = 0x8000000,
}

        public enum TPCANHandle : uint
        {
            PCAN_NONE = 0x00,
            PCAN_ISA = 0x21,
            PCAN_USB = 0x51
        }


        // Import PCAN-Basic functions
        [DllImport(PcanBasicDll, EntryPoint = "CAN_Initialize")]
        public static extern uint CAN_Initialize(uint Channel, ushort Btr0Btr1, uint HwType, uint IOPort, ushort Interrupt);
        public const ushort PCAN_BAUD_500K = 0x1C1;

        [DllImport(PcanBasicDll, EntryPoint = "CAN_InitializeFD")]
        public static extern TPCANStatus InitializeFD(TPCANHandle Channel,TPCANBitrateFD BitrateFD);

        [DllImport(PcanBasicDll, EntryPoint = "CAN_WriteFD")]
        public static extern TPCANStatus CAN_WriteFD(TPCANHandle Channel, ref TPCANMsgFD Message);

        [DllImport(PcanBasicDll, EntryPoint = "CAN_Uninitialize")]
        public static extern TPCANStatus CAN_Uninitialize(TPCANHandle Channel);


        [DllImport(PcanBasicDll, EntryPoint = "CAN_Write")]
        public static extern uint CAN_Write(uint Channel, ref TPCANMsg Message);

        [DllImport(PcanBasicDll, EntryPoint = "CAN_Read")]
        public static extern uint CAN_Read(uint Channel, out TPCANMsg Message, IntPtr Timestamp);

        [DllImport(PcanBasicDll, EntryPoint = "CAN_ReadFD")]
        public static extern TPCANStatus CAN_ReadFD(TPCANHandle Channel, out TPCANMsgFD Message, out nint timestamp);

        [DllImport(PcanBasicDll, EntryPoint = "CAN_Uninitialize")]
        public static extern uint CAN_Uninitialize(uint Channel);
    }
}
