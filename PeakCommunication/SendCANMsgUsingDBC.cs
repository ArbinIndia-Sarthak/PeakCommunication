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

            // Example message ID for demonstration
            uint messageID = 0x1C180010;

            // Send CAN message in a loop
            while (true)
            {
                SendFDCANMessage(channel, ReadDBC.finalContent, messageID);
                Thread.Sleep(20);
            }

            PcanBasic.CAN_Uninitialize(channel);
        }

        static void SendFDCANMessage(TPCANHandle Channel, string finalcontent, uint messageID)
        {
            // Find the message section in the finalcontent
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
                ushort value = (ushort)(0x10 + index++); // Assign dummy values

                for (int i = 0; i < length; i++)
                {
                    int bitPos = startBit + i, byteIndex = bitPos / 8, bitOffset = bitPos % 8;
                    if ((value & (1 << i)) != 0) data[byteIndex] |= (byte)(1 << bitOffset);
                }
                maxBit = Math.Max(maxBit, startBit + length);
            }

            dlc = dlc > 0 ? dlc : (int)Math.Ceiling(maxBit / 8.0);
            dlc = Math.Min(dlc, 64);

            // Prepare message
            PcanBasic.TPCANMsgFD message = new PcanBasic.TPCANMsgFD
            {
                ID = messageID,
                DLC = (byte)dlc,
                MSGTYPE = PcanBasic.TPCANMessageType.PCAN_MESSAGE_FD | PcanBasic.TPCANMessageType.PCAN_MESSAGE_BRS | PcanBasic.TPCANMessageType.PCAN_MESSAGE_EXTENDED,
                DATA = new byte[64]
            };
            Array.Copy(data, message.DATA, dlc);

             // Send the message
            var status = PcanBasic.CAN_WriteFD(Channel, ref message);
            if (status == PcanBasic.TPCANStatus.PCAN_ERROR_OK)
            {
                Console.WriteLine($"Sending Message ID: 0x{message.ID:X}");
                Console.WriteLine($"DLC: {message.DLC}");
                Console.Write("DATA: ");
                for (int i = 0; i < message.DLC; i++)
                {
                    Console.Write($"{message.DATA[i]:X2} "); // Print data in hex format
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"Failed to send message. Error: {status}");
            }
        }
    }
}
