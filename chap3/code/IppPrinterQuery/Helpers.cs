using System;
using System.IO;
using System.Text;


namespace GetPrinterAttributesRequest
{
    public class Helpers
    {
        public static short GetIppInt16(BinaryReader reader)
        {
            return ConvertToLittleEndian.Reverse(reader.ReadInt16());
        }

        public static int GetIppInt32(BinaryReader reader)
        {
            return ConvertToLittleEndian.Reverse(reader.ReadInt32());
        }

        public static void GetIppAttributes(BinaryReader reader)
        {
            string prevAttribute = null;

            do
            {
                var b = reader.ReadByte();
                var sectionTag = (DelimiterTag)b;

                switch (sectionTag)
                {    
                    case DelimiterTag.Reserved:
                    case DelimiterTag.OperationAttributesTag:
                    case DelimiterTag.JobAttributesTag:
                    case DelimiterTag.PrinterAttributesTag:
                    case DelimiterTag.UnsupportedAttributesTag:
                        break;
                    case DelimiterTag.EndOfAttributesTag: 
                        return;
                    default:

                        //print out attribute name and value(s)
                        var attribute = ReadAttribute((ValueTag)b, reader, prevAttribute);
                        prevAttribute = attribute;

                        if (prevAttribute == null && attribute == null)
                        {
                            throw new ArgumentException("Invalid IPP stream!");
                        }

                        break;
                }
            }
            while (true);
        }

        public static string ReadAttribute(ValueTag tag, BinaryReader stream, string previous)
        {
            var len = ConvertToLittleEndian.Reverse(stream.ReadInt16());
            var name = Encoding.ASCII.GetString(stream.ReadBytes(len));
            var value = ReadValue(stream, tag);
            var normalizedName = string.IsNullOrEmpty(name) && previous != null ? previous : name;

            if (string.IsNullOrEmpty(normalizedName))
            {
                throw new ArgumentException("0 length attribute name found not in a 1setOf");
            }

            //Write out the attribute name and value

            Console.WriteLine(string.Format("Attribute Name: {0}, Attribute Value {1}", name!=string.Empty?name:previous, value));

            return normalizedName;
        }

        public struct StringWithLanguage : IEquatable<StringWithLanguage>
        {
            public string Language { get; set; }
            public string Value { get; set; }
            public StringWithLanguage(string language, string value)
            {
                Language = language;
                Value = value;
            }

            public override string ToString()
            {
                return $"{Value} ({Language})";
            }

            public bool Equals(StringWithLanguage other)
            {
                return Language == other.Language && Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is StringWithLanguage other && Equals(other);
            }
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Language != null ? Language.GetHashCode() : 0) * 397) ^
                           (Value != null ? Value.GetHashCode() : 0);
                }
            }
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
        /// </summary>
        public enum ValueTag : byte
        {
            /// <summary>
            ///     unsupported
            /// </summary>
            Unsupported = 0x10,

            /// <summary>
            ///     unknown
            /// </summary>
            Unknown = 0x12,

            /// <summary>
            ///     no-value
            /// </summary>
            NoValue = 0x13,
            IntegerUnassigned20 = 0x20,

            /// <summary>
            ///     integer
            /// </summary>
            Integer = 0x21,

            /// <summary>
            ///     boolean
            /// </summary>
            Boolean = 0x22,

            /// <summary>
            ///     enum
            /// </summary>
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

            /// <summary>
            ///     octetString with an unspecified format
            /// </summary>
            OctetStringWithAnUnspecifiedFormat = 0x30,

            /// <summary>
            ///     dateTime
            /// </summary>
            DateTime = 0x31,

            /// <summary>
            ///     resolution
            /// </summary>
            Resolution = 0x32,

            /// <summary>
            ///     rangeOfInteger
            /// </summary>
            RangeOfInteger = 0x33,

            /// <summary>
            ///     begCollection
            /// </summary>
            BegCollection = 0x34,

            /// <summary>
            ///     textWithLanguage
            /// </summary>
            TextWithLanguage = 0x35,

            /// <summary>
            ///     nameWithLanguage
            /// </summary>
            NameWithLanguage = 0x36,

            /// <summary>
            ///     endCollection
            /// </summary>
            EndCollection = 0x37,
            OctetStringUnassigned38 = 0x38,
            OctetStringUnassigned39 = 0x39,
            OctetStringUnassigned3A = 0x3a,
            OctetStringUnassigned3B = 0x3b,
            OctetStringUnassigned3C = 0x3c,
            OctetStringUnassigned3D = 0x3d,
            OctetStringUnassigned3E = 0x3e,
            OctetStringUnassigned3F = 0x3f,

            StringUnassigned40 = 0x40,

            /// <summary>
            ///     textWithoutLanguage
            /// </summary>
            TextWithoutLanguage = 0x41,

            /// <summary>
            ///     nameWithoutLanguage
            /// </summary>
            NameWithoutLanguage = 0x42,

            StringUnassigned43 = 0x43,

            /// <summary>
            ///     keyword
            /// </summary>
            Keyword = 0x44,

            /// <summary>
            ///     uri
            /// </summary>
            Uri = 0x45,

            /// <summary>
            ///     uriScheme
            /// </summary>
            UriScheme = 0x46,

            /// <summary>
            ///     charset
            /// </summary>
            Charset = 0x47,

            /// <summary>
            ///     naturalLanguage
            /// </summary>
            NaturalLanguage = 0x48,

            /// <summary>
            ///     mimeMediaType
            /// </summary>
            MimeMediaType = 0x49,

            /// <summary>
            ///     memberAttrName
            /// </summary>
            MemberAttrName = 0x4a,
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
            StringUnassigned5F = 0x5f,
        }

        public static object ReadValue(BinaryReader stream, ValueTag tag)
        {
            //https://tools.ietf.org/html/rfc8010#section-3.5.2
            switch (tag)
            {
                case ValueTag.Unsupported:
                    return ReadNoValue(stream);
                case ValueTag.Unknown:
                    return ReadNoValue(stream);
                case ValueTag.NoValue:
                    return ReadNoValue(stream);
                case ValueTag.Integer:
                    return ReadInt(stream);
                case ValueTag.Enum:
                    return ReadInt(stream);
                case ValueTag.Boolean:
                    return ReadBool(stream);
                case ValueTag.OctetStringWithAnUnspecifiedFormat:
                    return ReadString(stream);
                case ValueTag.DateTime:
                    return ReadDateTimeOffset(stream);
                case ValueTag.Resolution:
                    return ReadResolution(stream);
                case ValueTag.RangeOfInteger:
                    return ReadRange(stream);
                case ValueTag.BegCollection:
                    //TODO: collection https://tools.ietf.org/html/rfc8010#section-3.1.6
                    return ReadString(stream);
                case ValueTag.TextWithLanguage:
                    return ReadStringWithLanguage(stream);
                case ValueTag.NameWithLanguage:
                    return ReadStringWithLanguage(stream);
                case ValueTag.EndCollection:
                    //TODO: collection https://tools.ietf.org/html/rfc8010#section-3.1.6
                    return ReadNoValue(stream);
                case ValueTag.TextWithoutLanguage:
                    return ReadString(stream);
                case ValueTag.NameWithoutLanguage:
                    return ReadString(stream);
                case ValueTag.Keyword:
                    return ReadString(stream);
                case ValueTag.Uri:
                    return ReadString(stream);
                case ValueTag.UriScheme:
                    return ReadString(stream);
                case ValueTag.Charset:
                    return ReadString(stream);
                case ValueTag.NaturalLanguage:
                    return ReadString(stream);
                case ValueTag.MimeMediaType:
                    return ReadString(stream);
                case ValueTag.MemberAttrName:
                    return ReadString(stream);
                case ValueTag.OctetStringUnassigned38:
                    return ReadString(stream);
                case ValueTag.OctetStringUnassigned39:
                    return ReadString(stream);
                case ValueTag.OctetStringUnassigned3A:
                    return ReadString(stream);
                case ValueTag.OctetStringUnassigned3B:
                    return ReadString(stream);
                case ValueTag.OctetStringUnassigned3C:
                    return ReadString(stream);
                case ValueTag.OctetStringUnassigned3D:
                    return ReadString(stream);
                case ValueTag.OctetStringUnassigned3E:
                    return ReadString(stream);
                case ValueTag.OctetStringUnassigned3F:
                    return ReadString(stream);
                case ValueTag.IntegerUnassigned20:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned24:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned25:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned26:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned27:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned28:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned29:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned2A:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned2B:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned2C:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned2D:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned2E:
                    return ReadInt(stream);
                case ValueTag.IntegerUnassigned2F:
                    return ReadInt(stream);
                case ValueTag.StringUnassigned40:
                    return ReadString(stream);
                case ValueTag.StringUnassigned43:
                    return ReadString(stream);
                case ValueTag.StringUnassigned4B:
                    return ReadString(stream);
                case ValueTag.StringUnassigned4C:
                    return ReadString(stream);
                case ValueTag.StringUnassigned4D:
                    return ReadString(stream);
                case ValueTag.StringUnassigned4E:
                    return ReadString(stream);
                case ValueTag.StringUnassigned4F:
                    return ReadString(stream);
                case ValueTag.StringUnassigned50:
                    return ReadString(stream);
                case ValueTag.StringUnassigned51:
                    return ReadString(stream);
                case ValueTag.StringUnassigned52:
                    return ReadString(stream);
                case ValueTag.StringUnassigned53:
                    return ReadString(stream);
                case ValueTag.StringUnassigned54:
                    return ReadString(stream);
                case ValueTag.StringUnassigned55:
                    return ReadString(stream);
                case ValueTag.StringUnassigned56:
                    return ReadString(stream);
                case ValueTag.StringUnassigned57:
                    return ReadString(stream);
                case ValueTag.StringUnassigned58:
                    return ReadString(stream);
                case ValueTag.StringUnassigned59:
                    return ReadString(stream);
                case ValueTag.StringUnassigned5A:
                    return ReadString(stream);
                case ValueTag.StringUnassigned5B:
                    return ReadString(stream);
                case ValueTag.StringUnassigned5C:
                    return ReadString(stream);
                case ValueTag.StringUnassigned5D:
                    return ReadString(stream);
                case ValueTag.StringUnassigned5E:
                    return ReadString(stream);
                case ValueTag.StringUnassigned5F:
                    return ReadString(stream);
                default:
                    throw new Exception($"Ipp tag {tag} not supported");
            };
        }

        public static DateTimeOffset ReadDateTimeOffset(BinaryReader stream)
        {
            var length = ConvertToLittleEndian.Reverse(stream.ReadInt16());

            if (length != 11)
            {
                throw new ArgumentException($"Expected datetime value length: 11, actual :{length}");
            }

            var year = ConvertToLittleEndian.Reverse(stream.ReadInt16());
            var month = stream.ReadByte();
            var day = stream.ReadByte();
            var hour = stream.ReadByte();
            var minute = stream.ReadByte();
            var second = stream.ReadByte();
            var decisecond = stream.ReadByte();
            var plusMinus = stream.ReadByte();

            var offsetDir = plusMinus == Encoding.ASCII.GetBytes("+")[0] ? 1 :
                plusMinus == Encoding.ASCII.GetBytes("-")[0] ? -1 :
                throw new ArgumentException($"DateTime offset direction {plusMinus} not supported");
            var offsetHour = stream.ReadByte();
            var offsetMinute = stream.ReadByte();

            var dateTimeOffset = new DateTimeOffset(year,
                month,
                day,
                hour,
                minute,
                second,
                decisecond * 100,
                new TimeSpan(offsetHour * offsetDir, offsetMinute, 0));
            return dateTimeOffset;
        }

        public static Resolution ReadResolution(BinaryReader stream)
        {
            var length = ConvertToLittleEndian.Reverse(stream.ReadInt16());

            if (length != 9)
            {
                throw new ArgumentException($"Expected resolution value length: 9, actual :{length}");
            }

            var width = ConvertToLittleEndian.Reverse(stream.ReadInt32());
            var height = ConvertToLittleEndian.Reverse(stream.ReadInt32());
            var units = stream.ReadByte();
            return new Resolution(width, height, (ResolutionUnit)units);
        }

        public static StringWithLanguage ReadStringWithLanguage(BinaryReader stream)
        {
            var _ = ConvertToLittleEndian.Reverse(stream.ReadInt16());
            var language = ReadString(stream);
            var value = ReadString(stream);
            return new StringWithLanguage(language, value);
        }

        public static int ReadInt(BinaryReader stream)
        {
            var length = ConvertToLittleEndian.Reverse(stream.ReadInt16());

            if (length != 4)
            {
                throw new ArgumentException("Invalid Integer value");
            }

            var value = ConvertToLittleEndian.Reverse(stream.ReadInt32());
            return value;
        }

        public static NoValue ReadNoValue(BinaryReader stream)
        {
            var length = ConvertToLittleEndian.Reverse(stream.ReadInt16());

            if (length != 0)
            {
                throw new ArgumentException("Invalid NoValue value");
            }

            return new NoValue();
        }

        public static Range ReadRange(BinaryReader stream)
        {
            var length = ConvertToLittleEndian.Reverse(stream.ReadInt16());

            if (length != 8)
            {
                throw new ArgumentException("Invalid Range value");
            }

            var lower = ConvertToLittleEndian.Reverse(stream.ReadInt32());
            var upper = ConvertToLittleEndian.Reverse(stream.ReadInt32());
            return new Range(lower, upper);
        }

        public struct Range
        {
            public int Low { get; }

            public int High { get; }

            public Range(int l, int h)
            {
                Low = l;
                High = h;
            }
            public override string ToString()
            {
                return string.Format("{0} - {1}", Low, High);
            }
        }

        public static string ReadString(BinaryReader stream)
        {
            var len = ConvertToLittleEndian.Reverse(stream.ReadInt16());
            return Encoding.ASCII.GetString(stream.ReadBytes(len));
        }
        public static bool ReadBool(BinaryReader stream)
        {
            var length = ConvertToLittleEndian.Reverse(stream.ReadInt16());

            if (length != 1)
            {
                throw new ArgumentException("Invalid boolean Value");
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

            throw new ArgumentException(string.Format("Boolean value {0} not supported", value));
        }

        public enum ResolutionUnit
        {
            DotsPerInch = 3,
            DotsPerCm = 4,
        }

        public struct Resolution : IEquatable<Resolution>
        {
            public int Width { get; }

            public int Height { get; }

            public ResolutionUnit Units { get; }

            public Resolution(int width, int height, ResolutionUnit units)
            {
                Width = width;
                Height = height;
                Units = units;
            }

            public override string ToString()
            {
                return $"{Width}x{Height} ({(Units == ResolutionUnit.DotsPerInch ? "dpi" : Units == ResolutionUnit.DotsPerCm ? "dpcm" : "unknown")})";
            }

            public bool Equals(Resolution other)
            {
                return Width == other.Width && Height == other.Height && Units == other.Units;
            }

            public override bool Equals(object obj)
            {
                return obj is Resolution other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Width;
                    hashCode = (hashCode * 397) ^ Height;
                    hashCode = (hashCode * 397) ^ (int)Units;
                    return hashCode;
                }
            }
        }

        public struct NoValue : IEquatable<NoValue>
        {
            public override string ToString()
            {
                return "no value";
            }

            public bool Equals(NoValue other)
            {
                return true;
            }

            public override bool Equals(object obj)
            {
                return obj is NoValue other && Equals(other);
            }

            public override int GetHashCode()
            {
                return 0;
            }

            public static NoValue Instance = new NoValue();
        }

    }
}
