using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peak.Can.Basic.BackwardCompatibility;
using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class ReceiveCANFD
    {
        public class ReceiveCanMessage
        {
            public static void InitialiseCANReceiver()
            {
                var channel = TPCANHandle.PCAN_USB;  // Change to the proper channel
                string bitrateFD = "f_clock_mhz=20, nom_brp=4, nom_tseg1=6, nom_tseg2=3, nom_sjw=1, data_brp=2, data_tseg1=3, data_tseg2=1, data_sjw=1";

                PcanBasic.TPCANStatus status1 = PcanBasic.InitializeFD(channel, bitrateFD);

                if (status1 != PcanBasic.TPCANStatus.PCAN_ERROR_OK)
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
                PcanBasic.TPCANMsgFD message;
                nint timestamp;
                //message.DLC = 64;
                PcanBasic.TPCANStatus status = PcanBasic.CAN_ReadFD(Channel, out message, out timestamp);

                if (status == PcanBasic.TPCANStatus.PCAN_ERROR_OK)
                {
                    Console.WriteLine("Message received!");
                    Console.WriteLine("ID: 0x" + message.ID.ToString("X"));
                    Console.WriteLine("DLC: " + message.DLC);
                    Console.Write("Data: ");

                    for (int i = 0; i < 64; i++)
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
