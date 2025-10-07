using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetPrinterAttributesRequest
{
    public class RequestHelpers
    {

        public static byte[] CreateAttribute(byte tag, string name, string value, bool bFirstOf)
        {
            var nameBytes = Encoding.UTF8.GetBytes(name);
            var valueBytes = Encoding.UTF8.GetBytes(value);

            // Convert nameBytes Integer16 length to 2 bytes, placing the MSB on the left (first) while the LSB on the right (second) for big-endan formatting, 
            // then add these 2 bytes to the byte array. Concat the nameBytes UTF-8 formatted bytes to the byte array as well. Repeat again for valueBytes argument.
            

            if (bFirstOf)
            {
                return new byte[] { tag } // Attribute tag
                    .Concat(new byte[] { (byte)(nameBytes.Length >> 8 & 0xFF), (byte)(nameBytes.Length & 0xFF) }) // Name length
                    .Concat(nameBytes) // Name
                    .Concat(new byte[] { (byte)(valueBytes.Length >> 8 & 0xFF), (byte)(valueBytes.Length & 0xFF) }) // Value length
                    .Concat(valueBytes) // Value
                    .ToArray();
            }
            else //If this is the (2nd or more) of a 1stOf list, then set the Name length to 0x00 and 0x00 in the byte stream
            {
                return new byte[] { tag } // Attribute tag
                    .Concat(new byte[] { (byte)(0x00), (byte)(0x00) })
                    .Concat(new byte[] { (byte)(valueBytes.Length >> 8 & 0xFF), (byte)(valueBytes.Length & 0xFF) }) // Value length
                    .Concat(valueBytes) // Value
                    .ToArray();
            }
        }
    }
}

