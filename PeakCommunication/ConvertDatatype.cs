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
            if (data == null || length != 8)
            {
                Console.WriteLine("Invalid data length. Expected 8 bytes.");
                return;
            }

            Console.WriteLine("Converted Data:");

            // First 2 bytes as Int16 (short)
            short int16Value = BitConverter.ToInt16(data, 0);
            Console.WriteLine($"- First 2 bytes as Int16: {int16Value}");

            // Next 2 bytes as UInt16 (unsigned short)
            ushort uint16Value = BitConverter.ToUInt16(data, 2);
            Console.WriteLine($"- Next 2 bytes as UInt16: {uint16Value}");

            /*// Next 2 bytes as signed Int16
            short signedInt16Value = BitConverter.ToInt16(data, 4);
            Console.WriteLine($"- Next 2 bytes as Signed Int16: {signedInt16Value}");*/

            // Last 2 bytes as Float (Requires 4 bytes, so we take from index 6)
            float floatValue = BitConverter.ToSingle(data, 4);
            Console.WriteLine($"- Last 4 bytes as Float: {floatValue}");

            Console.WriteLine();
        }
    }   
}
