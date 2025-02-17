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

            message.ID = 0X313; // Standard CAN ID (11-bit)
            message.DLC = 8;
            message.MSGTYPE = PcanBasic.TPCANMessageType.PCAN_MESSAGE_FD | PcanBasic.TPCANMessageType.PCAN_MESSAGE_BRS;

            //int intValue = 12345;
            //uint uintValue = 54321;
            //short signedValue = -1234;
            //float floatValue = 12.34f;
            //byte[] intBytes = BitConverter.GetBytes(intValue);
            //byte[] uintBytes = BitConverter.GetBytes(uintValue);
            //byte[] signedBytes = BitConverter.GetBytes(signedValue);
            //byte[] floatBytes = BitConverter.GetBytes(floatValue);
            //if (!BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(intBytes);
            //    Array.Reverse(uintBytes);
            //    Array.Reverse(signedBytes);
            //    Array.Reverse(floatBytes);
            //}


            // Set CAN data (example: 8-byte payload)
            message.DATA = new byte[8] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x09, 0x08, 0x00 };
           // Array.Copy(intBytes, 0, message.DATA, 0, 2);
            //Array.Copy(uintBytes, 0, message.DATA, 2, 2);   // 2 bytes for unsigned int
            //Array.Copy(signedBytes, 0, message.DATA, 4, 2); // 2 bytes for signed int
            //Array.Copy(floatBytes, 0, message.DATA, 4, 4);


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