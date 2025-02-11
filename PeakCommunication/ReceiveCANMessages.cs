using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class ReceiveCAN
    {
        public class ReceiveCanMessage
        {
            public static void InitialiseCANReceiver()
            {
                var channel = TPCANHandle.PCAN_USB;  // Change to the proper channel
                PcanBasic.TPCANBaudrate bitrate = PcanBasic.TPCANBaudrate.PCAN_BAUD_500K;

                uint status1 = PcanBasic.CAN_Initialize(channel, bitrate, 0, 0, 0);

                if (status1 != (uint)PcanBasic.TPCANStatus.PCAN_ERROR_OK)
                {
                    Console.WriteLine("Error initializing PCAN: " + status1);
                    return;
                }
                Console.WriteLine("PCAN receiver initialized successfully!");

                // Start receiving CAN messages
                while (true)
                {
                    ReceiveCANMessage(channel);
                    Thread.Sleep(20);
                }

                // Uninitialize PCAN after use
                CAN_Uninitialize(channel);
            }

            static void ReceiveCANMessage(TPCANHandle Channel)
            {
                PcanBasic.TPCANMsg message;
                nint timestamp = 0;
                message.DLC = 8;
                uint status = PcanBasic.CAN_Read(Channel, out message, timestamp);

                if (status == (uint)PcanBasic.TPCANStatus.PCAN_ERROR_OK)
                {
                    Console.WriteLine("Message received!");
                    Console.WriteLine("ID: 0x" + message.ID.ToString("X"));
                    Console.WriteLine("DLC: " + message.DLC);
                    Console.Write("Data: ");

                    for (int i = 0; i < message.DLC; i++)
                    {
                        Console.Write(message.DATA[i].ToString("X2") + " ");
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Receive Error: 0x" + status.ToString("X"));
                }
            }
        }
    }
}

