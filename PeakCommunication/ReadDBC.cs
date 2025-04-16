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
        public static string finalContent;

        public static object filteredMessages { get; private set; }

        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static string ReadDBCFile()
        {
            string dbcFilePath = @"C:\ArbinSoftware\merged_instancing.dbc"; // Corrected path

            if (!File.Exists(dbcFilePath))
            {
                Console.WriteLine($"DBC file not found at: {dbcFilePath}");
                return string.Empty;
            }

            // Create an instance of the DBC parser


            // Parse the DBC file
            //Dbc dbc = Parser.ParseFromPath(dbcFilePath);
            Dbc dbc = Parser.Parse(File.ReadAllText(dbcFilePath));


            if (dbc == null)
            {
                Console.WriteLine("Failed to parse DBC file.");
                return string.Empty;
            }

            // To check the data in file is available

            Console.WriteLine("Reading DBC file content...");
            string dbcRawContent = File.ReadAllText(dbcFilePath);
            Console.WriteLine(dbcRawContent.Substring(0, Math.Min(1500, dbcRawContent.Length))); // Print first 1500 chars

            StringBuilder contents = new StringBuilder();

            // Iterate through all messages in the DBC file

            try
            {
                foreach (var valueTable in dbc.GlobalProperties)

                    foreach (var message in dbc.Messages)
                    {
                        Console.WriteLine($"Message: 0x{message.ID.ToString("X")}, Name: {message.Name}, DLC: {message.DLC}, Comment: {message.Comment}");


                        string messageInfo = $@"Message: 0x{message.ID.ToString("X")}, Name: {message.Name}, DLC: {message.DLC}, Comment: {message.Comment}";
                        contents.AppendLine(messageInfo);

                        // Iterate through all signals in the message
                        foreach (var signal in message.Signals)
                        {
                            Console.WriteLine($@" Signal Name: {signal.Name}
                            StartBit: {signal.StartBit}
                            Length: {signal.Length}
                            Factor: {signal.Factor}
                            Offset: {signal.Offset}
                            ByteOrder: {signal.ByteOrder}
                            ValueType: {signal.ValueType}
                            ");


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

            finalContent = contents.ToString();

            string txtFilePath = @"C:\ArbinSoftware\Detailed Messages and signals.txt"; // new file path

            try
            {
                // Check if the file already exists
                if (File.Exists(txtFilePath))
                {
                    Console.WriteLine("File already exists at: " + txtFilePath);

                    // Overwrite the file with new contents
                    File.WriteAllText(txtFilePath, finalContent);

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
                        return finalContent;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error opening file: " + ex.Message);
                    }
                }
                else
                {
                    // Create and write to the file
                    File.WriteAllText(txtFilePath, finalContent);
                    Console.WriteLine("File created and data written successfully.");

                    // Open the file
                    Process.Start(new ProcessStartInfo(txtFilePath) { UseShellExecute = true });

                    return finalContent;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return finalContent;
        }


        //public static string ReadDBCFile()
        //{
        //    string dbcFilePath = @"C:\ArbinSoftware\merged_instancing.dbc"; // Corrected path
        //    var originalLines = File.ReadAllLines(dbcFilePath);
        //    var filteredLines = new List<string>();

        //    foreach (var line in originalLines)
        //    {
        //        if (line.StartsWith("BO_"))
        //        {
        //            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //            if (parts.Length >= 2 && uint.TryParse(parts[1], out uint messageId))
        //            {
        //                if (messageId > int.MaxValue)
        //                {
        //                    Console.WriteLine($"Skipping message with ID {messageId} (too large for parser).");
        //                    continue; // Skip this message
        //                }
        //            }
        //        }

        //        filteredLines.Add(line);
        //    }

        //    var tempFilteredPath = Path.Combine(Path.GetDirectoryName(dbcFilePath), "filtered.dbc");
        //    File.WriteAllLines(tempFilteredPath, filteredLines);

        //    // Parse the cleaned-up DBC file
        //    var dbc = Parser.Parse(File.ReadAllText(tempFilteredPath));

        //    return null;

        //}

        //public static string ReadDBCFile()
        //{
        //    string dbcFilePath = @"C:\ArbinSoftware\merged_instancing.dbc"; // Corrected path
        //    var originalLines = File.ReadAllLines(dbcFilePath);
        //    var filteredLines = new List<string>();

            

        //    foreach (var line in originalLines)
        //    {
        //        if (line.StartsWith("BO_"))
        //        {
        //            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //            if (parts.Length >= 2 && ulong.TryParse(parts[1], out ulong messageId))
        //            {
        //                if (messageId > uint.MaxValue)
        //                {
        //                    Console.WriteLine($"Skipping message ID {messageId} (too large for parser).");
        //                    continue;
        //                }
        //            }

        //            if (line.StartsWith("SG_"))
        //            {
        //                parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //                if (parts.Length > 2 && int.TryParse(parts[2], out int startBit))
        //                {
        //                    if (startBit > 63) // Filter invalid large start bits
        //                    {
        //                        Console.WriteLine($"Skipping signal with start bit {startBit} (too large)");
        //                        continue;
        //                    }
        //                }
        //            }
        //        }

        //        filteredLines.Add(line);
        //        Console.WriteLine(filteredLines);
        //    }

        //    var tempFilteredPath = Path.Combine(Path.GetDirectoryName(dbcFilePath), "filtered.dbc");
        //    File.WriteAllLines(tempFilteredPath, filteredLines);

        //    // Parse the cleaned-up DBC file
        //    var dbc = Parser.Parse(File.ReadAllText(tempFilteredPath));

        //    return null;
        //}
    }
}
