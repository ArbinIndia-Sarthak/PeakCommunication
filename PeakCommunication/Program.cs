using System;
using System.IO;
using DbcParserLib;

class Program
{
    static void Main(string[] args)
    {
        string dbcFilePath = @"C:\ArbinSoftware\amp_debug_v18.dbc"; // Corrected path

        if (!File.Exists(dbcFilePath))
        {
            Console.WriteLine($"DBC file not found at: {dbcFilePath}");
            return;
        }

        // Create an instance of the DBC parser
        

        // Parse the DBC file
        Dbc dbc = Parser.Parse(dbcFilePath);

        if (dbc == null)
        {
            Console.WriteLine("Failed to parse DBC file.");
            return;
        }

        Console.WriteLine("Reading DBC file content...");
        string dbcRawContent = File.ReadAllText(dbcFilePath);
        Console.WriteLine(dbcRawContent.Substring(0, Math.Min(500, dbcRawContent.Length))); // Print first 500 chars


        // Iterate through all messages in the DBC file

        try
        {
            foreach (var message in dbc.Messages)
            {
                Console.WriteLine($"Message: {message.ID}, Name: {message.Name}, DLC: {message.DLC}");

                // Iterate through all signals in the message
                foreach (var signal in message.Signals)
                {
                    Console.WriteLine($"  Signal: {signal.Name}, StartBit: {signal.StartBit}, Length: {signal.Length}, Factor: {signal.Factor}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error " + ex);
        }
            
    }
}
