using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.IO;
using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class SendCanMsgUsingDBC
    {
        public string dbcContent = ReadDBC.finalContent;

        public static void InitialiseCANMessage()
        {
            var channel = TPCANHandle.PCAN_USB;  // Change to the proper channel
            string bitrateFD = "f_clock_mhz=20, nom_brp=4, nom_tseg1=6, nom_tseg2=3, nom_sjw=1, data_brp=2, data_tseg1=3, data_tseg2=1, data_sjw=1";

            var status1 = PcanBasic.InitializeFD(channel, bitrateFD);
            if (status1 != PcanBasic.TPCANStatus.PCAN_ERROR_OK)
            {
                Console.WriteLine("Error initializing PCAN: " + status1);
                return;
            }
            Console.WriteLine("PCAN initialized successfully!");

            // List of message IDs to send simultaneously
            uint[] messageIDs = { 0x19100010, 0x17C00010, 0x16600010 };

            // Start a thread for each message ID
            List<Thread> threads = new List<Thread>();
            foreach (var messageID in messageIDs)
            {
                Thread thread = new Thread(() => SendFDCANMessages(channel, ReadDBC.finalContent, messageID));
                thread.IsBackground = true;
                thread.Start();
                threads.Add(thread);
            }

            // Keep the main thread alive
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
            PcanBasic.CAN_Uninitialize(channel);
            Console.WriteLine("PCAN Uninitialized.");
        }

        static void SendFDCANMessages(TPCANHandle Channel, string finalcontent, uint messageID)
        {
            try
            {
                while (true)
                {
                    SendFDCANMessage(Channel, finalcontent, messageID);
                    Thread.Sleep(20);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in thread for Message ID {messageID}: {ex.Message}");
            }
        }

        static void SendFDCANMessage(TPCANHandle Channel, string finalcontent, uint messageID)
        {
            try
            {
                string messagePattern = $"Message: 0x{messageID:X}";
                int messageIndex = finalcontent.IndexOf(messagePattern);
                if (messageIndex == -1) return;

                string messageBlock = finalcontent.Substring(messageIndex);
                int nextMessageIndex = finalcontent.IndexOf("Message:", messageIndex + messagePattern.Length);
                messageBlock = nextMessageIndex != -1 ? messageBlock.Substring(0, nextMessageIndex - messageIndex) : messageBlock;

                int dlc = Regex.Match(messageBlock, @"DLC:\s*(\d+)") is Match dlcMatch && dlcMatch.Success
                    ? int.Parse(dlcMatch.Groups[1].Value)
                    : 0;

                var matches = Regex.Matches(messageBlock, @"StartBit: (\d+)\s+Length: (\d+)");
                byte[] data = new byte[64];

                int maxBit = 0, index = 0;
                foreach (Match match in matches)
                {
                    int startBit = int.Parse(match.Groups[1].Value);
                    int length = int.Parse(match.Groups[2].Value);
                    ushort value = (ushort)(0x10 + index++);

                    for (int i = 0; i < length; i++)
                    {
                        int bitPos = startBit + i, byteIndex = bitPos / 8, bitOffset = bitPos % 8;
                        if ((value & (1 << i)) != 0) data[byteIndex] |= (byte)(1 << bitOffset);
                    }
                    maxBit = Math.Max(maxBit, startBit + length);
                }

                dlc = dlc > 0 ? dlc : (int)Math.Ceiling(maxBit / 8.0);
                dlc = Math.Min(dlc, 64);

                PcanBasic.TPCANMsgFD message = new PcanBasic.TPCANMsgFD
                {
                    ID = messageID,
                    DLC = (byte)dlc,
                    MSGTYPE = PcanBasic.TPCANMessageType.PCAN_MESSAGE_FD | PcanBasic.TPCANMessageType.PCAN_MESSAGE_BRS | PcanBasic.TPCANMessageType.PCAN_MESSAGE_EXTENDED,
                    DATA = new byte[64]
                };
                Array.Copy(data, message.DATA, dlc);

                var status = PcanBasic.CAN_WriteFD(Channel, ref message);
                if (status == PcanBasic.TPCANStatus.PCAN_ERROR_OK)
                {
                    Console.WriteLine($"Sent Message ID: 0x{message.ID:X}");
                    Console.WriteLine($"DLC: {message.DLC}");
                    Console.Write("DATA: ");
                    for (int i = 0; i < message.DLC; i++)
                    {
                        Console.Write($"{message.DATA[i]:X2} ");
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine($"Failed to send message. Error: {status}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in sending message {messageID}: {ex.Message}");
            }
        }
    }

}
