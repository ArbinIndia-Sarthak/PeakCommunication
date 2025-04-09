using DbcParserLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class CanFDReceiver1
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
                ReadCANFDMessage(channel);
                Thread.Sleep(20);
            }
        }

        static void ReadCANFDMessage(TPCANHandle Channel)
        {
            TPCANMsgFD message = new TPCANMsgFD();
            nint timestamp;

            var status = PcanBasic.CAN_ReadFD(Channel, out message, out timestamp);
            if (status == PcanBasic.TPCANStatus.PCAN_ERROR_OK)
            {
                Console.WriteLine($"\nReceived Message ID: 0x{message.ID:X}");
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
                        var signalInfo = GetSignalDetails(message.ID, signal);
                        if (signalInfo != null)
                        {
                            int rawValue = ExtractSignalData(message.DATA, signalInfo.StartBit, signalInfo.Length, signalInfo.ByteOrder);
                            Console.WriteLine($"  - {signal} | StartBit: {signalInfo.StartBit}, Length: {signalInfo.Length}, Raw Value: {rawValue}");
                        }
                        else
                        {
                            Console.WriteLine($"  - {signal}: No detailed info found in DBC.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"No matching signal found for Message ID: 0x{message.ID:X}");
                }
            }
            else if (status != PcanBasic.TPCANStatus.PCAN_ERROR_QRCVEMPTY)
            {
                Console.WriteLine($"Error receiving message: {status}");
            }
        }

        static SignalInfo GetSignalDetails(uint messageID, string signalName)
        {
            Dbc dbc = Parser.ParseFromPath(@"C:\\ArbinSoftware\\amp_debug_v18.dbc");
            if (dbc == null)
            {
                Console.WriteLine("Failed to parse DBC file.");
                return null;
            }

            var message = dbc.Messages.FirstOrDefault(m => m.ID == messageID);
            if (message == null)
            {
                Console.WriteLine($"Message ID 0x{messageID:X} not found in DBC.");
                return null;
            }

            var signal = message.Signals.FirstOrDefault(s => s.Name == signalName);
            if (signal == null)
            {
                Console.WriteLine($"Signal {signalName} not found in message 0x{messageID:X}.");
                return null;
            }

            return new SignalInfo
            {
                StartBit = signal.StartBit,
                Length = signal.Length,
                ByteOrder = signal.ByteOrder
            };
        }

        static int ExtractSignalData(byte[] data, int startBit, int length, int byteOrder)
        {
            int rawValue = 0;

            for (int i = 0; i < length; i++)
            {
                int bitPosition = startBit + i;
                int byteIndex = bitPosition / 8;
                int bitInByte = bitPosition % 8;

                if ((data[byteIndex] & (1 << bitInByte)) != 0)
                {
                    rawValue |= (1 << i);
                }
            }

            if (byteOrder == 0) // Big-endian
            {
                rawValue = ReverseBits(rawValue, length);
            }

            return rawValue;
        }

        static int ReverseBits(int value, int bitCount)
        {
            int reversed = 0;
            for (int i = 0; i < bitCount; i++)
            {
                if ((value & (1 << i)) != 0)
                {
                    reversed |= (1 << (bitCount - 1 - i));
                }
            }
            return reversed;
        }

        class SignalInfo
        {
            public int StartBit { get; set; }
            public int Length { get; set; }
            public int ByteOrder { get; set; }
        }
    }
}
