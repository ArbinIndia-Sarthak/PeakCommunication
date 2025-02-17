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
            uint messageID = 0x1E800010;

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
            if (messageIndex == -1)
            {
                Console.WriteLine($"Message ID {messageID} not found in DBC content.");
                return;
            }

            // Extract signals from the message
            string messageBlock = finalcontent.Substring(messageIndex);
            int nextMessageIndex = finalcontent.IndexOf("Message:", messageIndex + messagePattern.Length);
            messageBlock = nextMessageIndex != -1 ? messageBlock.Substring(0, nextMessageIndex - messageIndex) : messageBlock;

            // Calculate total bit length based on signals
            int maxBit = 0;
            var signalPattern = @"StartBit: (\d+)\s+Length: (\d+)";
            var matches = Regex.Matches(messageBlock, signalPattern);
            foreach (Match match in matches)
            {
                int startBit = int.Parse(match.Groups[1].Value);
                int length = int.Parse(match.Groups[2].Value);
                int signalEnd = startBit + length;
                if (signalEnd > maxBit) maxBit = signalEnd;
            }

            // Calculate DLC: (maxBit / 8) rounded up
            int dlc = (int)Math.Ceiling(maxBit / 8.0);
            dlc = dlc > 64 ? 64 : dlc; // Limit DLC to 64 bytes for CAN FD

            // Prepare message
            PcanBasic.TPCANMsgFD message = new PcanBasic.TPCANMsgFD
            {
                ID = messageID,
                DLC = (byte)dlc,
                MSGTYPE = PcanBasic.TPCANMessageType.PCAN_MESSAGE_FD | PcanBasic.TPCANMessageType.PCAN_MESSAGE_BRS,
                DATA = new byte[dlc]
            };

            // Populate data (dummy pattern for illustration)
            for (int i = 0; i < dlc; i++)
            {
                message.DATA[i] = (byte)(i + 1);
            }

            // Send the message
            var status = PcanBasic.CAN_WriteFD(Channel, ref message);
            if (status == PcanBasic.TPCANStatus.PCAN_ERROR_OK)
            {
                Console.WriteLine($"Message ID {messageID} sent successfully with DLC {dlc}");
            }
            else
            {
                Console.WriteLine($"Failed to send message. Error: {status}");
            }
        }
    }
}
