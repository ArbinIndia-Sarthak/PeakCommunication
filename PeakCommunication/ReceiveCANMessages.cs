using Peak.Can.Basic.BackwardCompatibility;
using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class ReceiveCANMessages
    {
        public static void InitialiseCANReceiver()
        {
            var channel = TPCANHandle.PCAN_USB; // Change to the proper channel
            ushort bitrate = PcanBasic.PCAN_BAUD_500K;

//            // Initialize the channel with 500 kbps baud rate
            uint status = PcanBasic.CAN_Initialize(channel, bitrate, 0, 0, 0);

            if (status != 0)
            {
                Console.WriteLine("Error initializing PCAN: " + status);
                return;
            }
            Console.WriteLine("PCAN initialized successfully for receiving!");

            // Start receiving messages
            ReceiveMessages(channel);

            // Uninitialize PCAN after use
            PcanBasic.CAN_Uninitialize(channel);
        }

        static void ReceiveMessages(uint channel)
        {
            PcanBasic.TPCANMsg message;
            nint timestamp = 0;
            //message.TYPE = 4;
            message.LEN = 16;

            while (true)
            {
                uint status = PcanBasic.CAN_Read(channel, out message, timestamp);

                if (status == 0) // No error
                {
                    Console.WriteLine("Received CAN message:");
                    Console.WriteLine($"ID: {message.ID:X}");  // Print the ID in hexadecimal format
                    Console.WriteLine($"Length: {message.LEN}"); // Print the message length
                    Console.Write("Data: ");

                    // Print the message data in hexadecimal format
                    foreach (byte b in message.DATA)
                    {
                        Console.Write($"{b:X2} ");
                    }
                    Console.WriteLine("\nTimestamp: " + timestamp + " µs");
                }
                else if (status != (uint)TPCANStatus.PCAN_ERROR_QRCVEMPTY)
                {
                    Console.WriteLine("Error reading CAN message: " + status);
                }

                Thread.Sleep(100); // Small delay to prevent CPU overuse
            }
        }

    }
}
