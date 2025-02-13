using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class SendCANMessageFD
    {
        public static void InitialiseCANMessage()
        {
            var channel = TPCANHandle.PCAN_USB;  // Change to the proper channel
                                                 // ushort bitrate = PcanBasic.PCAN_BAUD_500K;
                                                 // Nominal Bitrate: 500 kbps, Data Bitrate: 2 Mbps
            string bitrateFD = "f_clock_mhz=20, nom_brp=4, nom_tseg1=6, nom_tseg2=3, nom_sjw=1, data_brp=2, data_tseg1=3, data_tseg2=1, data_sjw=1";

            PcanBasic.TPCANStatus status1 = PcanBasic.InitializeFD(channel, bitrateFD);

            // Initialize the channel with 500 kbps baud rate
            //uint status = PcanBasic.CAN_Initialize(channel, bitrate, 0, 0, 0);

            if (status1 != PcanBasic.TPCANStatus.PCAN_ERROR_OK)
            {
                Console.WriteLine("Error initializing PCAN: " + status1);
                return;
            }
            Console.WriteLine("PCAN initialized successfully!");

            // Send CAN message
            while (true)
            {
                SendFDCANMessage(channel);
                Thread.Sleep(20);
            }

            // Uninitialize PCAN after use
            PcanBasic.CAN_Uninitialize(channel);
        }


        static void SendFDCANMessage(TPCANHandle Channel)
        {

            // Create a CAN message
            PcanBasic.TPCANMsgFD message = new PcanBasic.TPCANMsgFD();

            message.ID = 0x123; // Standard CAN ID (11-bit)
            message.DLC = 15;
            message.MSGTYPE = PcanBasic.TPCANMessageType.PCAN_MESSAGE_FD | PcanBasic.TPCANMessageType.PCAN_MESSAGE_BRS;

            // Set CAN data (example: 8-byte payload)
            message.DATA = new byte[64]
            {
                    0x12, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00,
                    0x12, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00,
                    0x12, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00,
                    0x12, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00,
                    0x12, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00,
                    0x12, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00,
                    0x12, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00,
                    0x12, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00
            };


            // Send the message
            PcanBasic.TPCANStatus status = PcanBasic.CAN_WriteFD(Channel, ref message);
            //uint status = PcanBasic.CAN_Write(channel, ref message);

            if (status == PcanBasic.TPCANStatus.PCAN_ERROR_OK)
            {
                Console.WriteLine("Message sent successfully!");
                for (int i = 0; i < 64; i++)
                {
                    Console.WriteLine($"Byte {i}: 0x{message.DATA[i]:X2}");
                }
            }
            else
            {
                Console.WriteLine("Initialization Error: 0x" + status.ToString("X"));
            }
        }
    }
}

