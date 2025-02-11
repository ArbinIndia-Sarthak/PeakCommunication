using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class SendCanMessage
    {
        public static void InitialiseCANMessage()
        {
            //var channel = (uint)TPCANHandle.PCAN_USB;
            var channel = TPCANHandle.PCAN_USB;
            TPCANBaudrate bitrate = PcanBasic.TPCANBaudrate.PCAN_BAUD_500K;

            uint status = PcanBasic.CAN_Initialize(channel, bitrate, 0, 0, 0);

            // Initialize the channel with 500 kbps baud rate
            //uint status = PcanBasic.CAN_Initialize(channel, bitrate, 0, 0, 0);

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
                Thread.Sleep(20);
            }

            // Uninitialize PCAN after use
            PcanBasic.CAN_Uninitialize(channel);
        }

        static void SendCANMessage(TPCANHandle Channel)
        {

            // Create a CAN message
            PcanBasic.TPCANMsg message = new PcanBasic.TPCANMsg();

            message.ID = 0x123; // Standard CAN ID (11-bit)
            message.DLC = 8;
            message.MSGTYPE = PcanBasic.TPCANMessageType.PCAN_MESSAGE_FD | PcanBasic.TPCANMessageType.PCAN_MESSAGE_BRS;

            int intValue = 12345;
            byte[] intBytes = BitConverter.GetBytes(intValue);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(intBytes);
            }


            // Set CAN data (example: 8-byte payload)
            message.DATA = new byte[8] /*{ 0x12, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00 }*/;
            Array.Copy(intBytes, 0, message.DATA, 0, 4);


            // Send the message
            uint status = PcanBasic.CAN_Write(Channel, ref message);
            //uint status = PcanBasic.CAN_Write(channel, ref message);

            if (status == 0)
            {
                Console.WriteLine("Message sent successfully!");
                for (int i = 0; i < message.DLC; i++)
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