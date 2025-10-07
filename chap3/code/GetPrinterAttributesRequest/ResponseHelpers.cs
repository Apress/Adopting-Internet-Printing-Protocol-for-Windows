using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using static GetPrinterAttributesRequest.ResponseHelpers;


namespace GetPrinterAttributesRequest
{
    public class ResponseHelpers
    {

        public static void GetIppAttributes(BinaryReader reader)
        {
            string prevAttribute = null;

            do
            {
                var b = reader.ReadByte();
                var delimiterTag = (DelimiterTag)b;

                switch (delimiterTag)
                {    
                    case DelimiterTag.Reserved:
                        Console.WriteLine("This is a reserved attribute tag");
                        break;
                    case DelimiterTag.OperationAttributesTag:
                        Console.WriteLine("This is an operations attribute tag");
                        break;
                    case DelimiterTag.JobAttributesTag:
                        Console.WriteLine("This is an job attribute tag");
                        break;
                    case DelimiterTag.PrinterAttributesTag:
                        Console.WriteLine("This is a printer attribute tag");
                        break;
                    case DelimiterTag.UnsupportedAttributesTag:
                        Console.WriteLine("This is an unsupported attribute tag");
                        break;
                        break;
                    case DelimiterTag.EndOfAttributesTag:
                        Console.WriteLine("This is an end of attributes tag");
                        return;
                    default:
                        //print out attribute type
                        Console.WriteLine(GetValueTagString((ValueTag)b));
                        //print out attribute name and value(s)
                        var attribute = GetAttribute((ValueTag)b, reader, prevAttribute);
                        prevAttribute = attribute;

                        if (prevAttribute == null && attribute == null)
                        {
                            throw new Exception("Invalid IPP response stream!");
                        }

                        break;
                }
            }
            while (true);
        }

        /// <summary>
        /// GetAttribute
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="stream"></param>
        /// <param name="previous"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetAttribute(ValueTag tag, BinaryReader stream, string previous)
        {
            var len = ByteOrder.Flip(stream.ReadInt16());
            var name = Encoding.UTF8.GetString(stream.ReadBytes(len));
            var value = GetValueTagValue(stream, tag);
            //If the name is null, then this is a 1setOf 
            var attName = string.IsNullOrEmpty(name) && previous != null ? previous : name;

            if (string.IsNullOrEmpty(attName))
            {
                throw new Exception("Attribute name not found in a 1setOf");
            }

            //Write out the attribute name and value

            Console.WriteLine(string.Format("Attribute Name: {0}, Attribute Value: {1}", name!=string.Empty?name:previous, value));

            return attName;
        }


        /// <summary>
        /// RFC8010 Delimiter-Tag values
        /// </summary>
        public enum DelimiterTag : byte
        {
            Reserved = 0x00,
            OperationAttributesTag = 0x01,
            JobAttributesTag = 0x02,
            EndOfAttributesTag = 0x03,
            PrinterAttributesTag = 0x04,
            UnsupportedAttributesTag = 0x05,
        }

        /// <summary>
        /// RFC8010 Value-Tag values
        ///
        /// </summary>
        public enum ValueTag : byte
        {
            Unsupported = 0x10,
            Unknown = 0x12,
            NoValue = 0x13,
            IntegerUnassigned20 = 0x20,
            Integer = 0x21,
            Boolean = 0x22,
            Enum = 0x23,
            IntegerUnassigned24 = 0x24,
            IntegerUnassigned25 = 0x25,
            IntegerUnassigned26 = 0x26,
            IntegerUnassigned27 = 0x27,
            IntegerUnassigned28 = 0x28,
            IntegerUnassigned29 = 0x29,
            IntegerUnassigned2A = 0x2A,
            IntegerUnassigned2B = 0x2B,
            IntegerUnassigned2C = 0x2C,
            IntegerUnassigned2D = 0x2D,
            IntegerUnassigned2E = 0x2E,
            IntegerUnassigned2F = 0x2F,
            OctetStringWithAnUnspecifiedFormat = 0x30,
            DateTime = 0x31,
            Resolution = 0x32,
            RangeOfInteger = 0x33,
            BegCollection = 0x34,                           //3.1.6
            TextWithLanguage = 0x35,
            NameWithLanguage = 0x36,
            EndCollection = 0x37,                           //3.1.6
            OctetStringUnassigned38 = 0x38,
            OctetStringUnassigned39 = 0x39,
            OctetStringUnassigned3A = 0x3a,
            OctetStringUnassigned3B = 0x3b,
            OctetStringUnassigned3C = 0x3c,
            OctetStringUnassigned3D = 0x3d,
            OctetStringUnassigned3E = 0x3e,
            OctetStringUnassigned3F = 0x3f,
            StringUnassigned40 = 0x40,
            TextWithoutLanguage = 0x41,
            NameWithoutLanguage = 0x42,
            StringUnassigned43 = 0x43,
            Keyword = 0x44,
            Uri = 0x45,
            UriScheme = 0x46,
            Charset = 0x47,
            NaturalLanguage = 0x48,
            MimeMediaType = 0x49,
            MemberAttrName = 0x4a,                          //3.1.7
            StringUnassigned4B = 0x4b,
            StringUnassigned4C = 0x4c,
            StringUnassigned4D = 0x4d,
            StringUnassigned4E = 0x4e,
            StringUnassigned4F = 0x4f,
            StringUnassigned50 = 0x50,
            StringUnassigned51 = 0x51,
            StringUnassigned52 = 0x52,
            StringUnassigned53 = 0x53,
            StringUnassigned54 = 0x54,
            StringUnassigned55 = 0x55,
            StringUnassigned56 = 0x56,
            StringUnassigned57 = 0x57,
            StringUnassigned58 = 0x58,
            StringUnassigned59 = 0x59,
            StringUnassigned5A = 0x5a,
            StringUnassigned5B = 0x5b,
            StringUnassigned5C = 0x5c,
            StringUnassigned5D = 0x5d,
            StringUnassigned5E = 0x5e,
            StringUnassigned5F = 0x5f
        }

        /// <summary>
        /// GetValueTagValue
        /// 
        /// </summary>
        /// <param name="stream">The BinaryReader object that holds the byte stream</param>
        /// <param name="vt">The value tag</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static object GetValueTagValue(BinaryReader stream, ValueTag vt)
        {
            switch (vt)
            {
                case ValueTag.Unsupported:
                    return GetNoValue(stream);
                case ValueTag.Unknown:
                    return GetNoValue(stream);
                case ValueTag.NoValue:
                    return GetNoValue(stream);
                case ValueTag.Integer:
                    return GetInt(stream);
                case ValueTag.Enum:
                    return GetInt(stream);
                case ValueTag.Boolean:
                    return GetBool(stream);
                case ValueTag.OctetStringWithAnUnspecifiedFormat:
                    return GetString(stream);
                case ValueTag.DateTime:
                    return GetDateTimeOffset(stream);
                case ValueTag.Resolution:
                    return GetResolution(stream);
                case ValueTag.RangeOfInteger:
                    return GetRange(stream);
                case ValueTag.BegCollection:
                    Console.WriteLine("----Begin Collection (0x34)----");
                    return GetString(stream);
                case ValueTag.TextWithLanguage:
                    return GetStringWithLanguage(stream);
                case ValueTag.NameWithLanguage:
                    return GetStringWithLanguage(stream);
                case ValueTag.EndCollection:
                    Console.WriteLine("----End Collection (0x37)----");
                    return GetNoValue(stream);
                case ValueTag.TextWithoutLanguage:
                    return GetString(stream);
                case ValueTag.NameWithoutLanguage:
                    return GetString(stream);
                case ValueTag.Keyword:
                    return GetString(stream);
                case ValueTag.Uri:
                    return GetString(stream);
                case ValueTag.UriScheme:
                    return GetString(stream);
                case ValueTag.Charset:
                    return GetString(stream);
                case ValueTag.NaturalLanguage:
                    return GetString(stream);
                case ValueTag.MimeMediaType:
                    return GetString(stream);
                case ValueTag.MemberAttrName:
                    Console.WriteLine("----Member Attribute (0x4a)----");
                    return GetString(stream);
                case ValueTag.OctetStringUnassigned38:
                    return GetString(stream);
                case ValueTag.OctetStringUnassigned39:
                    return GetString(stream);
                case ValueTag.OctetStringUnassigned3A:
                    return GetString(stream);
                case ValueTag.OctetStringUnassigned3B:
                    return GetString(stream);
                case ValueTag.OctetStringUnassigned3C:
                    return GetString(stream);
                case ValueTag.OctetStringUnassigned3D:
                    return GetString(stream);
                case ValueTag.OctetStringUnassigned3E:
                    return GetString(stream);
                case ValueTag.OctetStringUnassigned3F:
                    return GetString(stream);
                case ValueTag.IntegerUnassigned20:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned24:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned25:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned26:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned27:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned28:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned29:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned2A:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned2B:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned2C:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned2D:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned2E:
                    return GetInt(stream);
                case ValueTag.IntegerUnassigned2F:
                    return GetInt(stream);
                case ValueTag.StringUnassigned40:
                    return GetString(stream);
                case ValueTag.StringUnassigned43:
                    return GetString(stream);
                case ValueTag.StringUnassigned4B:
                    return GetString(stream);
                case ValueTag.StringUnassigned4C:
                    return GetString(stream);
                case ValueTag.StringUnassigned4D:
                    return GetString(stream);
                case ValueTag.StringUnassigned4E:
                    return GetString(stream);
                case ValueTag.StringUnassigned4F:
                    return GetString(stream);
                case ValueTag.StringUnassigned50:
                    return GetString(stream);
                case ValueTag.StringUnassigned51:
                    return GetString(stream);
                case ValueTag.StringUnassigned52:
                    return GetString(stream);
                case ValueTag.StringUnassigned53:
                    return GetString(stream);
                case ValueTag.StringUnassigned54:
                    return GetString(stream);
                case ValueTag.StringUnassigned55:
                    return GetString(stream);
                case ValueTag.StringUnassigned56:
                    return GetString(stream);
                case ValueTag.StringUnassigned57:
                    return GetString(stream);
                case ValueTag.StringUnassigned58:
                    return GetString(stream);
                case ValueTag.StringUnassigned59:
                    return GetString(stream);
                case ValueTag.StringUnassigned5A:
                    return GetString(stream);
                case ValueTag.StringUnassigned5B:
                    return GetString(stream);
                case ValueTag.StringUnassigned5C:
                    return GetString(stream);
                case ValueTag.StringUnassigned5D:
                    return GetString(stream);
                case ValueTag.StringUnassigned5E:
                    return GetString(stream);
                case ValueTag.StringUnassigned5F:
                    return GetString(stream);
                default:
                    throw new Exception(string.Format("Invalid tag {0}", vt));
            };
        }


        /// <summary>
        /// GetValueTagString
        /// 
        /// Print out the value tag string
        /// </summary>
        /// <param name="vt">The value tag/param>
        /// <returns></returns>
        public static string GetValueTagString(ValueTag vt)
        {
            if((vt >= ValueTag.IntegerUnassigned20) && (vt <= ValueTag.IntegerUnassigned2F))
            {
                return "Value Tag: IntegerUnassigned";
            }
            else if((vt >= ValueTag.OctetStringUnassigned38) && (vt <= ValueTag.OctetStringUnassigned3F))
            {
                return "Value Tag: OctetStringUnassigned";
            }
            else if ((vt >= ValueTag.StringUnassigned40) && (vt <= ValueTag.StringUnassigned5F))
            {
                return "Value Tag: StringUnassigned";
            }
            switch (vt)
            {
                case ValueTag.Unsupported:
                    return "Value Tag: Unsupported";
                case ValueTag.Unknown:
                    return "Value Tag: Unknown";
                case ValueTag.NoValue:
                    return "Value Tag: NoValue";
                case ValueTag.Integer:
                    return "Value Tag: Integer";
                case ValueTag.Enum:
                    return "Value Tag: Enum";
                case ValueTag.Boolean:
                    return "Value Tag: Boolean";
                case ValueTag.OctetStringWithAnUnspecifiedFormat:
                    return "Value Tag: OctetStringWithAnUnspecifiedFormat";
                case ValueTag.DateTime:
                    return "Value Tag: DateTime";
                case ValueTag.Resolution:
                    return "Value Tag: Resolution";
                case ValueTag.RangeOfInteger:
                    return "Value Tag: RangeOfInteger";
                case ValueTag.BegCollection:
                    return "Value Tag: BegCollection";
                case ValueTag.TextWithLanguage:
                    return "Value Tag: TextWithLanguage";
                case ValueTag.NameWithLanguage:
                    return "Value Tag: NameWithLanguage";
                case ValueTag.EndCollection:
                    return "Value Tag: EndCollection";
                case ValueTag.TextWithoutLanguage:
                    return "Value Tag: TextWithoutLanguage";
                case ValueTag.NameWithoutLanguage:
                    return "Value Tag: NameWithoutLanguage";
                case ValueTag.Keyword:
                    return "Value Tag: Keyword";
                case ValueTag.Uri:
                    return "Value Tag: Uri";
                case ValueTag.UriScheme:
                    return "Value Tag: UriScheme";
                case ValueTag.Charset:
                    return "Value Tag: Charset";
                case ValueTag.NaturalLanguage:
                    return "Value Tag: NaturalLanguage";
                case ValueTag.MimeMediaType:
                    return "Value Tag: MimeMediaType";
                case ValueTag.MemberAttrName:
                    return "Value Tag: MemberAttrName";
                default:
                    return "Value Tag: INVALID";
            }
        }

        /// <summary>
        /// GetDateTimeOffset
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DateTimeOffset GetDateTimeOffset(BinaryReader stream)
        {
            int len = 0;

            if ((len = ByteOrder.Flip(stream.ReadInt16())) != 11)
            {
                throw new Exception("Invalid DateTime attribute length");
            }

            byte[] dtArray = new byte[11];

            //read 11 bytes into byte array
            stream.Read(dtArray, 0, 11);

            // Extract the year, flipping the int order for bytes 0 and 1
            int year = (dtArray[0] << 8) | (dtArray[1]);

            int month = dtArray[2];
            int day = dtArray[3];
            int hour = dtArray[4];
            int minute = dtArray[5];
            int second = dtArray[6];
            int deciSecond = dtArray[7];

            var dateTimeOffset = new DateTimeOffset(year, month, day, hour, minute, second, deciSecond * 100, new TimeSpan(0,0,0));
            return dateTimeOffset;
        }


        /// <summary>
        /// GetStringWithLanguage
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static StringWithLanguage GetStringWithLanguage(BinaryReader stream)
        {
            var language = GetString(stream);
            var value = GetString(stream);
            return new StringWithLanguage(language, value);
        }

        public static int GetInt(BinaryReader stream)
        {
            var length = ByteOrder.Flip(stream.ReadInt16());

            if (length != 4)
            {
                throw new Exception("Invalid Integer value");
            }

            var value = ByteOrder.Flip(stream.ReadInt32());
            return value;
        }

        /// <summary>
        /// GetNoValue
        /// 
        /// Ref: Use of unknown and no-value attribute value tag
        /// Ref: https://www.pwg.org/archives/ipp/2011/016909.html
        /// Semantically-speaking "unknown" and "no-value" do mean different things:
        /// 1. unknown: There is a value but we don't know what it is.
        /// 2. no-value: We know there is no value.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetNoValue(BinaryReader stream)
        {
            var length = ByteOrder.Flip(stream.ReadInt16());

            if (length != 0)
            {
                throw new Exception("Invalid NoValue value");
            }

            return "No known Value";
        }

        /// <summary>
        /// StringWithLanguage
        /// </summary>
        public struct StringWithLanguage
        {
            public string Language { get; set; }
            public string Value { get; set; }
            public StringWithLanguage(string l, string v)
            {
                Language = l;
                Value = v;
            }
        }

        public static Range GetRange(BinaryReader stream)
        {
            int len = 0;

            if ((len = ByteOrder.Flip(stream.ReadInt16())) != 8)
            {

                throw new Exception("Invalid Range value");
            }

            return Range.Parse(stream.ReadBytes(len));
        }

        public struct Range
        {
            public int Start { get; private set; }
            public int End { get; private set; }

            public static Range Parse(byte[] byteStream)
            {
                using (MemoryStream ms = new MemoryStream(byteStream))
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        Range thisRange = new Range();

                        // Read Start of Range (4 bytes)
                        thisRange.Start = ByteOrder.Flip(reader.ReadInt32());

                        // Read End of Range (4 bytes)
                        thisRange.End = ByteOrder.Flip(reader.ReadInt32());

                        return thisRange;
                    }
                }
            }

            public override string ToString()
            {
                return $"Range: {Start} - {End}";
            }
        }

        public static string GetString(BinaryReader stream)
        {
            var len = ByteOrder.Flip(stream.ReadInt16());
            return Encoding.ASCII.GetString(stream.ReadBytes(len));
        }

        public static bool GetBool(BinaryReader stream)
        {
            var length = ByteOrder.Flip(stream.ReadInt16());

            if (length != 1)
            {
                throw new Exception("Invalid boolean Value");
            }

            var value = stream.ReadByte();

            if (value == 0x00)
            {
                return false;
            }

            if (value == 0x01)
            {
                return true;
            }

            throw new Exception(string.Format("Boolean value {0} not supported", value));
        }

        public static Resolution GetResolution(BinaryReader stream)
        {
            int len = 0;

            if ((len = ByteOrder.Flip(stream.ReadInt16())) != 9)
            {
                throw new Exception("Invalid Resolution value length");
            }

            return Resolution.Parse(stream.ReadBytes(len)); 
        }

        public struct Resolution
        {
            public int CrossFeedResolution { get; set; }
            public int FeedResolution { get; set; }
            public ResolutionUnit Unit { get; set; }

            public enum ResolutionUnit
            {
                DotsPerInch = 3,
                DotsPerCentimeter = 4
            }

            public static Resolution Parse(byte[] byteStream)
            {
                using (MemoryStream ms = new MemoryStream(byteStream))
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        Resolution resolution = new Resolution();

                        // Read CrossFeedResolution (4 bytes)
                        resolution.CrossFeedResolution = ByteOrder.Flip(reader.ReadInt32());

                        // Read FeedResolution (4 bytes)
                        resolution.FeedResolution = ByteOrder.Flip(reader.ReadInt32());

                        // Read Unit (1 byte)
                        resolution.Unit = (ResolutionUnit)reader.ReadByte();

                        return resolution;
                    }
                }
            }

            public override string ToString()
            {
                return $"Resolution: {CrossFeedResolution} x {FeedResolution} {Unit}";
            }
        }

    }
}
