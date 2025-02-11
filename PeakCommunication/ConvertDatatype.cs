using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PeakCommunication
{
    public class ConvertDatatype
    {
        public static void ConvertCANData(byte[] data, int length)
        {
            if (length == 0 || data == null)
            {
                Console.WriteLine("No data received.");
                return;
            }

            Console.WriteLine("Converted Data:");

            // Convert to Integer (Assumes little-endian, modify as needed)
            if (length >= 4)
            {
                int intValue = BitConverter.ToInt32(data, 0);
                Console.WriteLine($"- Integer: {intValue}");
            }

            // Convert to Float
            if (length >= 4)
            {
                float floatValue = BitConverter.ToSingle(data, 0);
                Console.WriteLine($"- Float: {floatValue}");
            }

            // Convert to ASCII String
            string asciiString = Encoding.ASCII.GetString(data, 0, length);
            Console.WriteLine($"- ASCII String: {asciiString}");

            // Convert to Binary String
            string binaryString = string.Join(" ", Array.ConvertAll(data, b => Convert.ToString(b, 2).PadLeft(8, '0')));
            Console.WriteLine($"- Binary: {binaryString}");

            // Convert to Hex String
            string hexString = BitConverter.ToString(data, 0, length).Replace("-", " ");
            Console.WriteLine($"- Hex String: {hexString}");

            Console.WriteLine();
        }
    }   
}
