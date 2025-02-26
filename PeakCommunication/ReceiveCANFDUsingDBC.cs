using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class ReceiveCanMsgUsingDBC
    {
        private static Dictionary<uint, List<string>> messageSignalMapping = new();

        public static void InitializeDBCData()
        {
            string finalContent = ReadDBC.finalContent;
            if (string.IsNullOrWhiteSpace(finalContent))
            {
                Console.WriteLine("DBC content is empty. Cannot initialize message mapping.");
                return;
            }

            // Extract all messages and signals from DBC content
            string[] messages = finalContent.Split(new[] { "Message:" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string messageBlock in messages)
            {
                Match msgMatch = Regex.Match(messageBlock, @"0x([A-Fa-f0-9]+)");
                if (!msgMatch.Success || !uint.TryParse(msgMatch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out uint messageID))
                    continue;

                if (!messageSignalMapping.ContainsKey(messageID))
                    messageSignalMapping[messageID] = new List<string>();

                var signalMatches = Regex.Matches(messageBlock, @"Signal Name:\s*(.+?)\n");
                foreach (Match match in signalMatches)
                {
                    messageSignalMapping[messageID].Add(match.Groups[1].Value.Trim());
                }
            }
            ReceiveCANMessage();
        }

        public static void ReceiveCANMessage()
        {
            var channel = TPCANHandle.PCAN_USB;
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

                if (messageSignalMapping.TryGetValue(message.ID, out List<string> signalNames))
                {
                    Console.WriteLine("Matched Signals:");
                    foreach (var signal in signalNames)
                    {
                        Console.WriteLine($"  - {signal}");
                    }
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
