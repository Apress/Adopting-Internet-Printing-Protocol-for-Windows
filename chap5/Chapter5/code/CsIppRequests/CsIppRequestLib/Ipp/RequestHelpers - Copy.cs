using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CsIppRequestLib
{
    public static class RequestHelpers
    {

        /// <summary>
        /// CreatePrinterAttribute
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="bFirstOf"></param>
        /// <returns></returns>
        public static byte[] CreatePrinterAttribute(byte tag, string name, object value, bool bFirstOf)
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


            if (!bFirstOf)
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

        /// <summary>
        /// CreateJobAttribute
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="bFirstOf"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] CreateJobAttribute(byte tag, string name, object value, bool bFirstOf)
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
            else
            {
                throw new ArgumentException("Unsupported attribute value type");
            }


            if (bFirstOf)
            {
                return new byte[] { tag } // Attribute tag
                    .Concat(new byte[] { (byte)(nameBytes.Length >> 8 & 0xFF), (byte)(nameBytes.Length & 0xFF) }) // Name length
                    .Concat(nameBytes) // Name
                    .Concat(new byte[] { (byte)(valueBytes.Length >> 8 & 0xFF), (byte)(valueBytes.Length & 0xFF) }) // Value length
                    .Concat(valueBytes) // Value
                    .ToArray();
            }
            else // If this is the (2nd or more) of a 1stOf list, then set the Name length to 0x00 and 0x00 in the byte stream
            {
                return new byte[] { tag } // Attribute tag
                    .Concat(new byte[] { (byte)(0x00), (byte)(0x00) })
                    .Concat(new byte[] { (byte)(valueBytes.Length >> 8 & 0xFF), (byte)(valueBytes.Length & 0xFF) }) // Value length
                    .Concat(valueBytes) // Value
                    .ToArray();
            }            
        }

        /// <summary>
        /// CreateJobAttributesByteArray
        /// </summary>
        /// <param name="jas"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte[] CreateJobAttributesByteArray(Dictionary<string, List<object>> jas)
        {
            if(jas.Count == 0)
                return null;

            byte[] valueBytes = new byte[] { };
            List<byte[]> lstbytes = new List<byte[]>();


            foreach (var kvp in jas)
            {
                var nameBytes = Encoding.UTF8.GetBytes(kvp.Key.ToString());
                //get the tag here...
                byte tag = AttributeHelper.GetJobAttributeByte(kvp.Key.ToString());
                foreach (var value in kvp.Value)
                {
                    if (value.GetType() == typeof(List<object>))
                    {
                        List<object> objects = (List<object>)value;
                        object[] values = objects.ToArray();
                        if(values.Length == 2)
                        {
                            //Range
                            var lower = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt32(values[0])));
                            var upper = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt32(values[1])));
                            valueBytes = lower.Concat(upper).ToArray();   
                        }
                        else if(values.Length == 3)
                        {
                            //Resolution
                            var crossFeedRes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt32(values[0])));
                            var feedRes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt32(values[1])));
                            var unitVal = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToByte(values[2])));
                            valueBytes = crossFeedRes.Concat(feedRes).Concat(unitVal).ToArray();
                        }
                        else
                        {
                            throw new Exception($"Invalid or unrecognized object collection for Name {kvp.Key.ToString()}");
                        }
                    }
                    else
                    {
                        // singluar value object
                        if (value is string stringValue)
                        {
                            valueBytes = Encoding.UTF8.GetBytes(stringValue);
                        }
                        else if(value is int intValue)
                        {
                            valueBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(intValue));
                        }
                    }

                   var attrBytes =  new byte[] { tag } // Attribute tag
                  .Concat(new byte[] { (byte)(nameBytes.Length >> 8 & 0xFF), (byte)(nameBytes.Length & 0xFF) }) // Name length
                  .Concat(nameBytes) // Name
                  .Concat(new byte[] { (byte)(valueBytes.Length >> 8 & 0xFF), (byte)(valueBytes.Length & 0xFF) }) // Value length
                  .Concat(valueBytes) // Value
                  .ToArray();
                    
                    lstbytes.Add(attrBytes);   
                }
            }

            byte[] combiinedArrays = lstbytes.SelectMany(arr => arr).ToArray();
            return combiinedArrays;
        }


        public static byte[] GetOperationCode(string requestType)
        {
            switch (requestType.ToLower())
            {
                case "validate-job":
                    return new byte[] { 0x00, 0x04 };
                case "create-job":
                    return new byte[] { 0x00, 0x05 };
                case "send-document":
                    return new byte[] { 0x00, 0x06 };
                default:
                    throw new ArgumentException("Invalid IPP operation");
            }
        }
        ////////
        ///
        public static byte[] CreateCollectionJobAttribute(byte tag, string collectionName, string collectionString)
        {
            var collectionNameBytes = Encoding.UTF8.GetBytes(collectionName);
            var result = new List<byte>();

            // Begin collection
            result.Add(0x34); // begin-collection tag
            result.AddRange(new byte[] { (byte)(collectionNameBytes.Length >> 8 & 0xFF), (byte)(collectionNameBytes.Length & 0xFF) }); // Collection name length
            result.AddRange(collectionNameBytes); // Collection name
            result.AddRange(new byte[] { 0x00, 0x00 }); // Value length (begin-collection has no value)

            // Parse the collection string
            var memberAttributes = ParseCollectionString(collectionString);

            foreach (var (name, value) in memberAttributes)
            {
                var nameBytes = Encoding.UTF8.GetBytes(name);
                byte[] valueBytes;
                byte valueTag;

                if (value is string stringValue)
                {
                    valueBytes = Encoding.UTF8.GetBytes(stringValue);
                    valueTag = 0x41; // textWithoutLanguage
                }
                else if (value is int intValue)
                {
                    valueBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(intValue));
                    valueTag = 0x21; // integer
                }
                else
                {
                    throw new ArgumentException("Unsupported attribute value type");
                }

                // Member attribute
                result.Add(valueTag); // Member attribute tag
                result.AddRange(new byte[] { (byte)(nameBytes.Length >> 8 & 0xFF), (byte)(nameBytes.Length & 0xFF) }); // Member attribute name length
                result.AddRange(nameBytes); // Member attribute name
                result.AddRange(new byte[] { (byte)(valueBytes.Length >> 8 & 0xFF), (byte)(valueBytes.Length & 0xFF) }); // Member attribute value length
                result.AddRange(valueBytes); // Member attribute value
            }

            // End collection
            result.AddRange(new byte[] { 0x37, 0x00, 0x00, 0x00, 0x00 }); // end-collection

            return result.ToArray();
        }

        private static List<(string name, object value)> ParseCollectionString(string collectionString)
        {
            var memberAttributes = new List<(string name, object value)>();
            var regex = new Regex(@"\{([^{}]+)\}");
            var matches = regex.Matches(collectionString);

            foreach (Match match in matches)
            {
                var parts = match.Groups[1].Value.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    var name = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (int.TryParse(value, out int intValue))
                    {
                        memberAttributes.Add((name, intValue));
                    }
                    else
                    {
                        memberAttributes.Add((name, value));
                    }
                }
            }

            return memberAttributes;
        }
    }

}
