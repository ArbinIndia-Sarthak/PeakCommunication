using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class SendCanMessage
    {
        public static void InitialiseCANMessage() 
        {
            uint channel = PcanBasic.PCAN_USB; // Change to the proper channel
            ushort bitrate = PcanBasic.PCAN_BAUD_500K;

            // Initialize the channel with 500 kbps baud rate
            uint status = PcanBasic.CAN_Initialize(channel, bitrate, 0, 0, 0);

            if (status != 0)
            {
                Console.WriteLine("Error initializing PCAN: " + status);
                return;
            }
            Console.WriteLine("PCAN initialized successfully!");

            // Send CAN message
            while (true)
            {
                SendCANMessage(channel);
                Thread.Sleep(1000);
            }

            // Uninitialize PCAN after use
            PcanBasic.CAN_Uninitialize(channel);
        }

        static void SendCANMessage(uint channel)
        {
            // Create a CAN message
            TPCANMsg message = new TPCANMsg();
            message.ID = 0x123; // Standard CAN ID (11-bit)
            message.LEN = 8;    // Data length (max 8 bytes)
            message.TYPE = 0; // Standard frame

            // Set CAN data (example: 8-byte payload)
            message.DATA = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            // Send the message
            uint status = PcanBasic.CAN_Write(channel, ref message);

            if (status == 0)
            {
                Console.WriteLine("Message sent successfully!");
            }
            else
            {
                Console.WriteLine("Error sending message: " + status);
            }
        }
    }
    
}
