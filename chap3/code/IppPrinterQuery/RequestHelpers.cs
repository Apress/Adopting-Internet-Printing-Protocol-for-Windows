using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetPrinterAttributesRequest
{
    public class RequestHelpers
    {
        /// <summary>
        /// CreatePrinterAttribute
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] CreatePrinterAttribute(byte tag, string name, object value)
        {
            var nameBytes = Encoding.UTF8.GetBytes(name);
            byte[] valueBytes;

            if (value is string stringValue)
            {
                valueBytes = Encoding.UTF8.GetBytes(stringValue);
            }
            else if (value is int intValue)
            {
                valueBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(intValue));
            }
            else if (value is bool boolValue)
            {
                valueBytes = new byte[] { boolValue ? (byte)0x01 : (byte)0x00 };
            }
            else
            {
                throw new ArgumentException("Unsupported attribute value type");
            }

            // Convert nameBytes Integer16 length to 2 bytes, placing the MSB on the left (first) while the LSB on the right (second) for big-endan formatting, 
            // then add these 2 bytes to the byte array. Concat the nameBytes UTF-8 formatted bytes to the byte array as well. Repeat again for valueBytes argument.
            return new byte[] { tag } // Attribute tag
                .Concat(new byte[] { (byte)(nameBytes.Length >> 8 & 0xFF), (byte)(nameBytes.Length & 0xFF) }) // Name length
                .Concat(nameBytes) // Name
                .Concat(new byte[] { (byte)(valueBytes.Length >> 8 & 0xFF), (byte)(valueBytes.Length & 0xFF) }) // Value length
                .Concat(valueBytes) // Value
                .ToArray();
        }

    }
}

