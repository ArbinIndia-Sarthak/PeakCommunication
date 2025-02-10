using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class SendCanMessage
    {
        public static void InitialiseCANMessage()
        {
            var channel = (uint)TPCANHandle.PCAN_USB;  // Change to the proper channel
                                                       // ushort bitrate = PcanBasic.PCAN_BAUD_500K;
                                                       // Nominal Bitrate: 500 kbps, Data Bitrate: 2 Mbps
            ushort bitrate = PcanBasic.PCAN_BAUD_500K;

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

        static void SendCANMessage(uint Channel)
        {

            // Create a CAN message
            PcanBasic.TPCANMsgFD message = new PcanBasic.TPCANMsgFD();

            message.ID = 0x123; // Standard CAN ID (11-bit)
            message.DLC = 8;
            message.MSGTYPE = PcanBasic.TPCANMessageType.PCAN_MESSAGE_FD | PcanBasic.TPCANMessageType.PCAN_MESSAGE_BRS;

            // Set CAN data (example: 8-byte payload)
            message.DATA = new byte[8] { 0x12, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00 };


            // Send the message
            uint status = PcanBasic.CAN_WriteFD(Channel, ref message);
            //uint status = PcanBasic.CAN_Write(channel, ref message);

            if (status == 0)
            {
                Console.WriteLine("Message sent successfully!");
                for (int i = 0; i < message.DLC; i++)
                {
                    Console.WriteLine(message.DATA[i]);
                }
            }
            else
            {
                Console.WriteLine("Initialization Error: 0x" + status.ToString("X"));
            }
        }
    }

}
