using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CsIppRequestLib
{
    public static class RequestHelpers
    {
        public const int npos = -1;

        /// <summary>
        /// CreateIppAttribute
        /// Creates printer attributes
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="bFirstOf"></param>
        /// <returns></returns>
        public static byte[] CreateIppAttribute(byte tag, string name, object value)
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


        /// <summary>
        /// CreatePrinterAttribute
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
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
                    if (tag == (byte)AttributeHelper.ValueTag.BegCollection)
                    {
                        //get the byte array representation of this string collection
                        try
                        {
                            lstbytes.Add(CreateCollectionByteArray(kvp.Key.ToString(), value.ToString()));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error parsing collection attribute {kvp.Key.ToString()}, reason: {ex.Message}");
                        }
                    }
                    else
                    {
                        if (value.GetType() == typeof(List<object>))
                        {
                            List<object> objects = (List<object>)value;
                            object[] values = objects.ToArray();
                            if (values.Length == 2)
                            {
                                //Range
                                var lower = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt32(values[0])));
                                var upper = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt32(values[1])));
                                valueBytes = lower.Concat(upper).ToArray();
                            }
                            else if (values.Length == 3)
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
                            else if (value is int intValue)
                            {
                                valueBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(intValue));
                            }
                        }

                        var attrBytes = new byte[] { tag } // Attribute tag
                       .Concat(new byte[] { (byte)(nameBytes.Length >> 8 & 0xFF), (byte)(nameBytes.Length & 0xFF) }) // Name length
                       .Concat(nameBytes) // Name
                       .Concat(new byte[] { (byte)(valueBytes.Length >> 8 & 0xFF), (byte)(valueBytes.Length & 0xFF) }) // Value length
                       .Concat(valueBytes) // Value
                       .ToArray();
                        lstbytes.Add(attrBytes);
                    }
                }
            }

            byte[] combinedArrays = lstbytes.SelectMany(arr => arr).ToArray();
            return combinedArrays;
        }


        /// <summary>
        /// ToBigEndianBytes
        /// 
        /// Take a unsigned short value and return a byte array in Big-Endian format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static byte[] ToBigEndianBytes(ushort value)
        {
            byte highByte = (byte)(value >> 8); // Get the most significant byte of value
            byte lowByte = (byte)(value & 0xFF); // Mask the least significant byte of value
            return new byte[] { highByte, lowByte };
        }

        /// <summary>
        /// CreateCollectionByteArray
        /// 
        /// Take a collection name and a structured collection string and return a byte array 
        /// representation of them.
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="collectionString"></param>
        /// <returns></returns>
        public static byte[] CreateCollectionByteArray(string collectionName, string collectionString)
        {
            //create beginning and end of collection byte arrays
            byte[] EndCollectionByteArray = new byte[] { (byte)AttributeHelper.ValueTag.EndCollection, 0x00, 0x00, 0x00, 0x00 };
            byte[] BeginCollectionByteArray = new byte[] { (byte)AttributeHelper.ValueTag.BegCollection, 0x00, 0x00, 0x00, 0x00 };

            var CollByteArray = new List<byte[]>();
            var coll = ParseCollectionString(collectionString); // return object representation of collection string

            // ---- Outer collection header ----
            CollByteArray.Add(new[] { (byte)AttributeHelper.ValueTag.BegCollection });
            var collNameBytes = Encoding.UTF8.GetBytes(collectionName);
            CollByteArray.Add(ToBigEndianBytes((ushort)collNameBytes.Length));              // attribute name-length
            CollByteArray.Add(collNameBytes);                                               // attribute name
            CollByteArray.Add(ToBigEndianBytes(0));                                         // value-length = 0

            foreach (var memberAttr in coll) 
            {
                // --- memberAttrName record ---
                var memberNameBytes = Encoding.UTF8.GetBytes(memberAttr.Name);
                CollByteArray.Add(new[] { (byte)AttributeHelper.ValueTag.MemberAttrName });
                CollByteArray.Add(ToBigEndianBytes(0));                                     // name-length MUST be 0 in collections
                CollByteArray.Add(ToBigEndianBytes((ushort)memberNameBytes.Length));        // value-length = len(name)
                CollByteArray.Add(memberNameBytes);                                         // value = name

                if (memberAttr.IsCollection)
                {
                    // --- nested collection header ---
                    CollByteArray.Add(BeginCollectionByteArray);

                    // Emit submembers (memberAttrName + member-value)
                    foreach (var sub in memberAttr)
                    {
                        var subNameBytes = Encoding.UTF8.GetBytes(sub.Name);

                        // sub memberAttrName
                        CollByteArray.Add(new[] { (byte)AttributeHelper.ValueTag.MemberAttrName });
                        CollByteArray.Add(ToBigEndianBytes(0));
                        CollByteArray.Add(ToBigEndianBytes((ushort)subNameBytes.Length));
                        CollByteArray.Add(subNameBytes);

                        // sub member value
                        byte[] valBytes = GetByteValue(sub.ValueTag, sub.Value);
                        CollByteArray.Add(new[] { sub.ValueTag });
                        CollByteArray.Add(ToBigEndianBytes(0));
                        CollByteArray.Add(ToBigEndianBytes((ushort)valBytes.Length));
                        CollByteArray.Add(valBytes);
                    }

                    // --- close nested collection ---
                    CollByteArray.Add(EndCollectionByteArray);
                }
                else
                {
                    // --- scalar member value ---
                    byte[] valBytes = GetByteValue(memberAttr.ValueTag, memberAttr.Value);
                    CollByteArray.Add(new[] { memberAttr.ValueTag });
                    CollByteArray.Add(ToBigEndianBytes(0)); // name-length = 0
                    CollByteArray.Add(ToBigEndianBytes((ushort)valBytes.Length));
                    CollByteArray.Add(valBytes);
                }
            }

            // ---- Close outer collection
            CollByteArray.Add(EndCollectionByteArray);
            return CollByteArray.SelectMany(b => b).ToArray();
        }

        /// <summary>
        /// GetByteValue
        /// 
        /// Given a value tag and the object value, return the byte array representation.
        /// </summary>
        /// <param name="valueTag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        private static byte[] GetByteValue(byte valueTag, object value)
        {
            if (value is null)
            { 
                throw new InvalidOperationException("Null member value.");
            }

            switch (valueTag)
            {
                case (byte)AttributeHelper.ValueTag.Keyword:
                case (byte)AttributeHelper.ValueTag.NameWithoutLanguage:
                case (byte)AttributeHelper.ValueTag.TextWithoutLanguage:
                case (byte)AttributeHelper.ValueTag.Uri:
                    return (Encoding.UTF8.GetBytes((string)value));

                case (byte)AttributeHelper.ValueTag.Integer:
                    {
                        int i = value is int iv ? iv : int.Parse(value.ToString());
                        return (BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i)));
                    }

                case (byte)AttributeHelper.ValueTag.Boolean:
                    {
                        bool b = value is bool bv ? bv : bool.Parse(value.ToString());
                        return (new[] { b ? (byte)0x01 : (byte)0x00 });
                    }

                default:
                    throw new NotSupportedException($"Unhandled valueTag 0x{valueTag:X2}");
            }
        }

     

        /// <summary>
        /// InferType
        /// 
        /// Given a string value, try to infer the byte representation
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static byte InferType(string value)
        {
            byte[] ba = new byte[1];
            // Try to parse as integer
            if (int.TryParse(value, out _))
                return (byte)AttributeHelper.ValueTag.Integer;
            else if ((value.ToLower() == "false") || (value.ToLower() == "true"))
                return (byte)AttributeHelper.ValueTag.Boolean;
            else if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
                return (byte)AttributeHelper.ValueTag.Uri;
            else //punt to keyword
                return 0x44;
        }

        /// <summary>
        /// ParseCollectionString
        /// </summary>
        /// <param name="baseString"></param>
        /// <returns></returns>
        public static CollectionAttribute ParseCollectionString(string baseString)
        {
            CollectionAttribute ca = new CollectionAttribute();
            //First strip off the opening and closing '{' and '}'
            int pos = baseString.IndexOf('{');
            pos++;
            int end = baseString.LastIndexOf('}');
            string input = baseString.Substring(pos, end - pos);

            List<string> lstTemp =  SplitCollection(input);
            foreach (var element in lstTemp)
            {
                MemberAttribute ma = new MemberAttribute();
                ma.IsCollection = true; 
                if (IsCollection(element))
                {
                    CollectionItem ci = ParseCollection(element);
                    ma.Name = ci.CollectionName;
                    foreach (var item in ci.Items)
                    {
                        Member member = new Member();
                        member.Name = item.Key.ToString();
                        member.Value = item.Value.ToString();
                        member.ValueTag = InferType(item.Value.ToString());
                        ma.Members.Add(member);
                    }
                }
                else
                {
                    ma = getNonCollection(element);
                }
                ca.MemberAttributes.Add(ma);
            }

            return ca;
        }

        /// <summary>
        /// IsCollection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool IsCollection(string value)
        {
            int openCount = 0;
            int closeCount = 0;

            foreach (char c in value)
            {
                if (c == '{')
                    openCount++;
                else if (c == '}')
                    closeCount++;
            }

            return ((openCount == closeCount) && (openCount >= 1));
        }

        /// <summary>
        /// getNonCollection
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static MemberAttribute getNonCollection(string input)
        {
            string[] elements = input.Split('=');
            MemberAttribute ma = new MemberAttribute();
            ma.Name = elements[0];  
            ma.Value = elements[1];
            ma.ValueTag = InferType((string)elements[1]);
            ma.IsCollection = false;
            return ma;
        }

        
        public static CollectionItem ParseCollection(string input)
        {
            var result = new CollectionItem();

            if (string.IsNullOrWhiteSpace(input))
                return result;

            // Try to match "name={key=value,...}" format
            var colMatch = Regex.Match(input, @"^([^\=]+)=\{(.+)\}$");
            if (colMatch.Success)
            {
                result.CollectionName = colMatch.Groups[1].Value.Trim();
                string content = colMatch.Groups[2].Value;

                // Split by comma, but not inside braces
                int depth = 0;
                var sb = new System.Text.StringBuilder();
                var pairs = new List<string>();
                foreach (char c in content)
                {
                    if (c == '{') 
                        depth++;
                    if (c == '}') 
                        depth--;
                    if (c == ',' && depth == 0)
                    {
                        pairs.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                if (sb.Length > 0)
                    pairs.Add(sb.ToString());

                foreach (var pair in pairs)
                {
                    var kv = pair.Split(new[] { '=' }, 2);
                    if (kv.Length == 2)
                    {
                        result.Items.Add((kv[0].Trim(), kv[1].Trim()));
                    }
                }
                return result;
            }

            // Try to match "name{key=value}{key=value}" format
            var sizeMatch = Regex.Match(input, @"^([^\{]+)");
            if (sizeMatch.Success)
            {
                result.CollectionName = sizeMatch.Groups[1].Value.Trim();
                var itemMatches = Regex.Matches(input, @"\{([^=]+)=([^\}]+)\}");
                foreach (Match m in itemMatches)
                {
                    result.Items.Add((m.Groups[1].Value.Trim(), m.Groups[2].Value.Trim()));
                }
            }

            return result;
        }

       
        public static List<string> SplitCollection(string input)
        {
            List<string> lstStrings = new List<string>();
            //Look for the first ','
            int pos = input.IndexOf(',');
            if (pos == npos)
                throw new Exception("Invalid Collection Composition");
            else
            {
                lstStrings.Add(input.Substring(0, pos));
                pos++;
                lstStrings.Add(input.Substring(pos));
            }
           return lstStrings;
        }
        
    }

}
