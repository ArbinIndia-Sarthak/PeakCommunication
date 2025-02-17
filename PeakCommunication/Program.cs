using System;
using System.IO;
using DbcParserLib;
using Peak.Can.Basic;
using PeakCommunication;
using static PeakCommunication.PcanBasic;
using TPCANHandle = System.UInt16;

class Program
{
    static void Main(string[] args)
    {
        ReadDBC.ReadDBCFile();

        //SendCANMessageFD.InitialiseCANMessage();
        //SendCanMessage.InitialiseCANMessage();
        //ReceiveCAN.ReceiveCanMessage.InitialiseCANReceiver();
        //ReceiveCANFD.InitialiseCANReceiver();
        SendCanMsgUsingDBC.InitialiseCANMessage();
        
    }
}
