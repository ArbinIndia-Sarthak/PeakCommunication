using DbcParserLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PeakCommunication
{
    public class ReadDBC
    {
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public static void ReadDBCFile()
        {
            string dbcFilePath = @"C:\ArbinSoftware\amp_debug_v18.dbc"; // Corrected path

            if (!File.Exists(dbcFilePath))
            {
                Console.WriteLine($"DBC file not found at: {dbcFilePath}");
                return;
            }

            // Create an instance of the DBC parser


            // Parse the DBC file
            Dbc dbc = Parser.ParseFromPath(dbcFilePath);

            if (dbc == null)
            {
                Console.WriteLine("Failed to parse DBC file.");
                return;
            }

            // To check the data in file is available

            //Console.WriteLine("Reading DBC file content...");
            //string dbcRawContent = File.ReadAllText(dbcFilePath);
            //Console.WriteLine(dbcRawContent.Substring(0, Math.Min(500, dbcRawContent.Length))); // Print first 500 chars

            StringBuilder contents = new StringBuilder();

            // Iterate through all messages in the DBC file

            try
            {
                foreach (var message in dbc.Messages)
                {
                    string messageInfo = $@"Message: {message.ID}, Name: {message.Name}, DLC: {message.DLC}, Comment: {message.Comment}";
                    contents.AppendLine(messageInfo);

                    // Iterate through all signals in the message
                    foreach (var signal in message.Signals)
                    {
                        string signalInfo = $@" Signal Name: {signal.Name}
    StartBit: {signal.StartBit}
    Length: {signal.Length}
    Factor: {signal.Factor}
    Offset: {signal.Offset}
    ByteOrder: {signal.ByteOrder}
    ValueType: {signal.ValueType}
";
                        contents.AppendLine(signalInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex);
            }


            // print data in a .txt file

            string finalcontent = contents.ToString();

            string txtFilePath = @"C:\ArbinSoftware\Detailed Messages and signals.txt"; // new file path

            try
            {
                // Check if the file already exists
                if (File.Exists(txtFilePath))
                {
                    Console.WriteLine("File already exists at: " + txtFilePath);

                    // Overwrite the file with new contents
                    File.WriteAllText(txtFilePath, finalcontent);

                    //open the existing file and bring  in front
                    try
                    {
                        Process process = new Process();
                        process.StartInfo = new ProcessStartInfo(txtFilePath)
                        {
                            UseShellExecute = true
                        };
                        process.Start();

                        // Give time for the process to start
                        Thread.Sleep(500);

                        // Bring the file window to the front
                        IntPtr hWnd = process.MainWindowHandle;
                        if (hWnd != IntPtr.Zero)
                        {
                            SetForegroundWindow(hWnd);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error opening file: " + ex.Message);
                    }
                }
                else
                {
                    // Create and write to the file
                    File.WriteAllText(txtFilePath, finalcontent);
                    Console.WriteLine("File created and data written successfully.");

                    // Open the file
                    Process.Start(new ProcessStartInfo(txtFilePath) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }
    }
}
