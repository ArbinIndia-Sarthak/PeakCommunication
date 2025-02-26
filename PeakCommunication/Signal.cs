using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakCommunication
{
    public class Signal
    {
        public string Name { get; set; }
        public int StartBit { get; set; }
        public int Length { get; set; }
        public double Factor { get; set; }
        public double Offset { get; set; }
        public int ByteOrder { get; set; }
        public string ValueType { get; set; }

        public override string ToString()
        {
            return $"Signal Name: {Name}, StartBit: {StartBit}, Length: {Length}, Factor: {Factor}, Offset: {Offset}, ByteOrder: {ByteOrder}, ValueType: {ValueType}";
        }
    }

}
