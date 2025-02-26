using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using static PeakCommunication.PcanBasic;
using System.Threading.Channels;
using System.Text.RegularExpressions;

namespace PeakCommunication
{
    public class ReceiveCanMsgUsingDBC
    {
        private static Dictionary<uint, string> messageSignalMapping;

        public static void InitializeDBCData()
        {
            string finalContent = ReadDBC.finalContent;
            if (string.IsNullOrWhiteSpace(finalContent))
            {
                Console.WriteLine("DBC content is empty. Cannot initialize message mapping.");
                return;
            }

            messageSignalMapping = new Dictionary<uint, string>();
            string[] lines = finalContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            uint currentMessageID = 0;

            foreach (string line in lines)
            {
                if (line.StartsWith("Message:"))
                {
                    Match match = Regex.Match(line, @"Message:\s*0x([A-Fa-f0-9]+)");
                    if (match.Success && uint.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out uint msgID))
                    {
                        currentMessageID = msgID;
                    }
                }
                else if (line.StartsWith(" Signal Name:") && currentMessageID != 0)
                {
                    string signalName = line.Substring(line.IndexOf(":") + 1).Trim();
                    if (!messageSignalMapping.ContainsKey(currentMessageID))
                    {
                        messageSignalMapping[currentMessageID] = signalName;
                    }
                }
            }

            Console.WriteLine($"Loaded {messageSignalMapping.Count} message-to-signal mappings.");
            ReceiveCANMessage();
        }


        public static void ReceiveCANMessage()
        {
            var channel = TPCANHandle.PCAN_USB;  // Change to the appropriate channel
            string bitrateFD = "f_clock_mhz=20, nom_brp=4, nom_tseg1=6, nom_tseg2=3, nom_sjw=1, data_brp=2, data_tseg1=3, data_tseg2=1, data_sjw=1";

            var status = PcanBasic.InitializeFD(channel, bitrateFD);
            if (status != PcanBasic.TPCANStatus.PCAN_ERROR_OK)
            {
                Console.WriteLine("Error initializing PCAN: " + status);
                return;
            }
            Console.WriteLine("PCAN initialized successfully for receiving!");

            while (true)
            {
                ReadFDCANMessage(channel);
                Thread.Sleep(20);
            }

            PcanBasic.CAN_Uninitialize(channel);
        }

        static void ReadFDCANMessage(TPCANHandle Channel)
        {
            TPCANMsgFD message = new TPCANMsgFD();
            nint timestamp;

            var status = PcanBasic.CAN_ReadFD(Channel, out message, out timestamp);
            if (status == PcanBasic.TPCANStatus.PCAN_ERROR_OK)
            {
                Console.WriteLine($"Received Message ID: 0x{message.ID:X}");
                Console.WriteLine($"DLC: {message.DLC}");
                Console.Write("DATA: ");
                for (int i = 0; i < message.DLC; i++)
                {
                    Console.Write($"{message.DATA[i]:X2} ");
                }
                Console.WriteLine();

                if (messageSignalMapping.TryGetValue(message.ID, out string signalName))
                {
                    Console.WriteLine($"Matched Signal: {signalName}");
                }
                else
                {
                    Console.WriteLine($"No matching signal found for Message ID: 0x{message.ID:X}. Please check DBC file.");
                }
            }
            else if (status != PcanBasic.TPCANStatus.PCAN_ERROR_QRCVEMPTY)
            {
                Console.WriteLine($"Error receiving message: {status}");
            }
        }

    }
}
