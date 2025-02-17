using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PeakCommunication.PcanBasic;

namespace PeakCommunication
{
    public class SendCANMsgUsingDBC
    {
        private static Dictionary<uint, int> MessageDlcMap = new Dictionary<uint, int>();

        // Initialize CAN communication
        public static void InitialiseCANMessage()
        {
            var channel = TPCANHandle.PCAN_USB;
            string bitrateFD = "f_clock_mhz=20, nom_brp=4, nom_tseg1=6, nom_tseg2=3, nom_sjw=1, data_brp=2, data_tseg1=3, data_tseg2=1, data_sjw=1";

            TPCANStatus status = PcanBasic.InitializeFD(channel, bitrateFD);
            if (status != TPCANStatus.PCAN_ERROR_OK)
            {
                Console.WriteLine("Error initializing PCAN: " + status);
                return;
            }
            Console.WriteLine("PCAN initialized successfully!");

            // Populate DLC map from ReadDBC finalcontent
            PopulateDlcMap();

            while (true)
            {
                foreach (var messageId in MessageDlcMap.Keys)
                {
                    SendFDCANMessage(channel, messageId);
                }
                Thread.Sleep(20);
            }

            PcanBasic.CAN_Uninitialize(channel);
        }

        // Populate MessageDlcMap from ReadDBC finalcontent
        private static void PopulateDlcMap()
        {
            string content = ReadDBC.finalcontent;
            string[] lines = content.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("Message"))
                {
                    var parts = line.Split(',');
                    uint messageId = uint.Parse(parts[0].Split(':')[1].Trim());
                    int dlc = int.Parse(parts[2].Split(':')[1].Trim());
                    MessageDlcMap[messageId] = dlc;
                }
            }
        }
        // Send CAN FD message based on the parsed DBC data
        private static void SendFDCANMessage(TPCANHandle Channel, uint messageId)
        {
            int dlc = MessageDlcMap[messageId];

            TPCANMsgFD message = new TPCANMsgFD
            {
                ID = messageId,
                DLC = (byte)dlc,
                MSGTYPE = TPCANMessageType.PCAN_MESSAGE_FD | TPCANMessageType.PCAN_MESSAGE_BRS,
                DATA = new byte[dlc]
            };

            // Fill the data with sample values or dynamic signal values if needed
            for (int i = 0; i < dlc; i++)
            {
                message.DATA[i] = (byte)(i + 1);
            }

            TPCANStatus status = PcanBasic.CAN_WriteFD(Channel, ref message);
            if (status == TPCANStatus.PCAN_ERROR_OK)
            {
                Console.WriteLine($"Message ID 0x{messageId:X} with DLC {dlc} sent successfully!");
            }
            else
            {
                Console.WriteLine($"Error sending message ID 0x{messageId:X}: {status}");
            }
        }
    }
}
