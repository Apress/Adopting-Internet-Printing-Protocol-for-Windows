using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace CsIppRequestLib
{
    public static class ResponseHelpers
    {
        /// <summary>
        /// GetIppAttributesCollectionAsync
        /// 
        /// Similar to GetIppAttributesAsync, except designed to create a collection of searchable printer attributes for the 
        /// Get-Printer-Attribute request. 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <param name="_pac"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<int> GetIppAttributesCollectionAsync(Stream stream, IppRequest.RequestType type, IppAttributes _pac)
        {
            /*
            if ((type != IppRequest.RequestType.GET_PRINTER_ATTRIBUTES) && (type != IppRequest.RequestType.GET_JOBS))
            {
                throw new Exception($"GetIppAttributesCollectionAsync does not support request type: {type}");
            }
            */
            var jobIdContainer = new JobIdContainer { JobId = -1 };
            string prevAttribute = null;
            int _jobId = -1;
            byte[] buffer = new byte[1];
            int jobTagIndex = 0;

            do
            {
                if (stream.Position == stream.Length)
                    return _jobId;

                await stream.ReadAsync(buffer, 0, 1);
                var delimiterTag = (DelimiterTag)buffer[0];

                switch (delimiterTag)
                {
                    case DelimiterTag.Reserved:
                        break;
                    case DelimiterTag.OperationAttributesTag:
                        break;
                    case DelimiterTag.JobAttributesTag:
                        jobTagIndex++;
                        break;
                    case DelimiterTag.PrinterAttributesTag:
                        break;
                    case DelimiterTag.UnsupportedAttributesTag:
                        break;
                    case DelimiterTag.EndOfAttributesTag:
                        return -1;
                    default:
                        var attribute = await GetAttributeCollectionAsync((ValueTag)delimiterTag, stream, prevAttribute, type, _jobId, _pac, jobTagIndex, jobIdContainer);
                        prevAttribute = attribute;

                        if (attribute == "EXIT")
                            return jobIdContainer.JobId; 
                        break;
                }
            }
            while (true);
        }

        /// <summary>
        /// GetAttributeCollectionAsync
        /// 
        /// Called by GetIppAttributesCollectionAsync, similar to GetAttributeAsync, except designed to create a collection 
        /// of searchable printer attributes for the Get-Printer-Attribute request. .
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="stream"></param>
        /// <param name="previous"></param>
        /// <param name="type"></param>
        /// <param name="jobId"></param>
        /// <param name="_pac"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static async Task<string> GetAttributeCollectionAsync(ValueTag tag, Stream stream, string previous, IppRequest.RequestType type, int jobId, IppAttributes _pac, int jobTag, JobIdContainer jobIdContainer)
        {

            byte[] lenBuffer = new byte[2];
            await stream.ReadAsync(lenBuffer, 0, 2);
            int len = ByteOrder.Flip(BitConverter.ToInt16(lenBuffer, 0));

            byte[] nameBuffer = new byte[len];
            await stream.ReadAsync(nameBuffer, 0, len);
            string name = Encoding.UTF8.GetString(nameBuffer);

            var value = await GetValueTagValueAsync(stream, tag);

            // Convert enums to readable string
            if (tag == ValueTag.Enum)
            {
                if (Mappings.IsMapped(name != string.Empty ? name : previous) == true)
                {
                    if (Mappings.enumMappings.TryGetValue(name != string.Empty ? name : previous, out var attributeMappings))
                    {
                        if (attributeMappings.TryGetValue(Convert.ToInt32(value), out var enumStringValue))
                        {
                            value = enumStringValue;
                        }
                    }
                }
            }

            //If the name is null, then this is a 1setOf 
            var attName = string.IsNullOrEmpty(name) && previous != null ? previous : name;

            if (string.IsNullOrEmpty(attName))
            {
                throw new Exception("Attribute name not found in a 1setOf");
            }

            else
            {
                //Write out the attribute name and value
                string attributeName = name != string.Empty ? name : previous;
                IppAttribute pAttr = _pac.Find(attributeName);
                if (type == IppRequest.RequestType.GET_PRINTER_ATTRIBUTES)
                {
                    if (pAttr == null)
                    {
                        IppAttribute pa = new IppAttribute(attributeName, value.ToString(), (byte)tag);
                        _pac.AddAttribute(pa);
                    }
                    else
                    {
                        pAttr.AddAttributeValue(value.ToString());
                    }
                }
                else if (type == IppRequest.RequestType.GET_JOBS)
                {
                    IppAttribute pa = new IppAttribute(attributeName, value.ToString(), (byte)tag, jobTag);
                    _pac.AddJobAttribute(pa);
                }
                else if (type == IppRequest.RequestType.CREATE_JOB && name.ToLower() == "job-id")
                {
                    jobIdContainer.JobId = Convert.ToInt32(value);
                    return "EXIT";
                }
                else if (type == IppRequest.RequestType.GET_JOB_ATTRIBUTES)
                {
                    IppAttribute pa = new IppAttribute(attributeName, value.ToString(), (byte)tag, jobTag);
                    _pac.AddJobAttribute(pa);
                }
            }
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
            StringUnassigned5F = 0x5f,
        }


        /// <summary>
        /// GetValueTagValueAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="vt"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<object> GetValueTagValueAsync(Stream stream, ValueTag vt)
        {
            switch (vt)
            {
                case ValueTag.Unsupported:
                    return await GetNoValueAsync(stream);
                case ValueTag.Unknown:
                    return await GetNoValueAsync(stream);
                case ValueTag.NoValue:
                    return await GetNoValueAsync(stream);
                case ValueTag.Integer:
                    return await GetIntAsync(stream);
                case ValueTag.Enum:
                    return await GetIntAsync(stream);
                case ValueTag.Boolean:
                    return await GetBoolAsync(stream);
                case ValueTag.OctetStringWithAnUnspecifiedFormat:
                    return await GetStringAsync(stream);
                case ValueTag.DateTime:
                    return await GetDateTimeOffsetAsync(stream);
                case ValueTag.Resolution:
                    return await GetResolutionAsync(stream);
                case ValueTag.RangeOfInteger:
                    return await GetRangeAsync(stream);
                case ValueTag.BegCollection:
                    return await GetStringAsync(stream);
                case ValueTag.TextWithLanguage:
                    return await GetStringWithLanguageAsync(stream);
                case ValueTag.NameWithLanguage:
                    return await GetStringWithLanguageAsync(stream);
                case ValueTag.EndCollection:
                    return await GetNoValueAsync(stream);
                case ValueTag.TextWithoutLanguage:
                    return await GetStringAsync(stream);
                case ValueTag.NameWithoutLanguage:
                    return await GetStringAsync(stream);
                case ValueTag.Keyword:
                    return await GetStringAsync(stream);
                case ValueTag.Uri:
                    return await GetStringAsync(stream);
                case ValueTag.UriScheme:
                    return await GetStringAsync(stream);
                case ValueTag.Charset:
                    return await GetStringAsync(stream);
                case ValueTag.NaturalLanguage:
                    return await GetStringAsync(stream);
                case ValueTag.MimeMediaType:
                    return await GetStringAsync(stream);
                case ValueTag.MemberAttrName:
                    return await GetStringAsync(stream);
                case ValueTag.OctetStringUnassigned38:
                    return await GetStringAsync(stream);
                case ValueTag.OctetStringUnassigned39:
                    return await GetStringAsync(stream);
                case ValueTag.OctetStringUnassigned3A:
                    return await GetStringAsync(stream);
                case ValueTag.OctetStringUnassigned3B:
                    return await GetStringAsync(stream);
                case ValueTag.OctetStringUnassigned3C:
                    return await GetStringAsync(stream);
                case ValueTag.OctetStringUnassigned3D:
                    return await GetStringAsync(stream);
                case ValueTag.OctetStringUnassigned3E:
                    return await GetStringAsync(stream);
                case ValueTag.OctetStringUnassigned3F:
                    return await GetStringAsync(stream);
                case ValueTag.IntegerUnassigned20:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned24:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned25:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned26:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned27:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned28:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned29:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned2A:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned2B:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned2C:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned2D:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned2E:
                    return await GetIntAsync(stream);
                case ValueTag.IntegerUnassigned2F:
                    return await GetIntAsync(stream);
                case ValueTag.StringUnassigned40:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned43:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned4B:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned4C:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned4D:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned4E:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned4F:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned50:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned51:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned52:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned53:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned54:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned55:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned56:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned57:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned58:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned59:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned5A:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned5B:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned5C:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned5D:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned5E:
                    return await GetStringAsync(stream);
                case ValueTag.StringUnassigned5F:
                    return await GetStringAsync(stream);
                default:
                    throw new Exception(string.Format("Invalid tag {0}", vt));
            }
            ;
        }


        /// <summary>
        /// GetDateTimeOffsetAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<DateTimeOffset> GetDateTimeOffsetAsync(Stream stream)
        {
            int len = 0;

            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                if ((len = ByteOrder.Flip(reader.ReadInt16())) != 11)
                {
                    throw new Exception("Invalid DateTime attribute length");
                }

                byte[] dtArray = new byte[11];

                // Asynchronously read 11 bytes into the byte array
                await stream.ReadAsync(dtArray, 0, 11);

                // Extract the year, flipping the int order for bytes 0 and 1
                int year = (dtArray[0] << 8) | dtArray[1];

                int month = dtArray[2];
                int day = dtArray[3];
                int hour = dtArray[4];
                int minute = dtArray[5];
                int second = dtArray[6];
                int deciSecond = dtArray[7];

                var dateTimeOffset = new DateTimeOffset(year, month, day, hour, minute, second, deciSecond * 100, new TimeSpan(0, 0, 0));
                return dateTimeOffset;
            }
        }

       
        /// <summary>
        /// GetStringWithLanguageAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<StringWithLanguage> GetStringWithLanguageAsync(Stream stream)
        {
            //read 2 bytes of the encoding type..
            byte[] lenBuffer = new byte[2];
            await stream.ReadAsync(lenBuffer, 0, 2);
            int encoding = ByteOrder.Flip(BitConverter.ToInt16(lenBuffer, 0));

            //get language
            var language = await GetStringAsync(stream);

            //get value
            var value = await GetStringAsync(stream);
            return new StringWithLanguage(language, value);
        }
       

        /// <summary>
        /// GetIntAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<int> GetIntAsync(Stream stream)
        {
            byte[] lenBuffer = new byte[2];
            await stream.ReadAsync(lenBuffer, 0, 2);
            int length = ByteOrder.Flip(BitConverter.ToInt16(lenBuffer, 0));

            if (length != 4)
            {
                throw new Exception("Invalid Integer value");
            }

            byte[] intBuffer = new byte[length];
            await stream.ReadAsync(intBuffer, 0, 4);
            var value = ByteOrder.Flip(BitConverter.ToInt32(intBuffer, 0));
            return value;
        }


        /// <summary>
        /// GetNoValueAsync
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
        public static async Task<string> GetNoValueAsync(Stream stream)
        {
            byte[] lenBuffer = new byte[2];
            await stream.ReadAsync(lenBuffer, 0, 2);
            int length = ByteOrder.Flip(BitConverter.ToInt16(lenBuffer, 0));

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
            public override string ToString()
            {
                return ($"{Value}[{Language}]");
            }
        }

        /// <summary>
        /// GetRangeAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception> 
        public static async Task<Range> GetRangeAsync(Stream stream)
        {
            byte[] lenBuffer = new byte[2];
            await stream.ReadAsync(lenBuffer, 0, 2);
            int length = ByteOrder.Flip(BitConverter.ToInt16(lenBuffer, 0));
            if (length != 8)
            {
                throw new Exception("Invalid Range value");
            }

            byte[] rangeBuffer = new byte[length];
            await stream.ReadAsync(rangeBuffer, 0, length);
            return await Range.ParseAsync(rangeBuffer);
        }

        /// <summary>
        /// Range
        /// </summary>
        public struct Range
        {
            public int Start { get; private set; }
            public int End { get; private set; }

            public static async Task<Range> ParseAsync(byte[] byteStream)
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

        /// <summary>
        /// GetStringAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<string> GetStringAsync(Stream stream)
        {
            byte[] lenBuffer = new byte[2];
            await stream.ReadAsync(lenBuffer, 0, 2);
            int len = ByteOrder.Flip(BitConverter.ToInt16(lenBuffer, 0));

            byte[] stringBuffer = new byte[len];
            await stream.ReadAsync(stringBuffer, 0, len);
            return Encoding.UTF8.GetString(stringBuffer);
        }

        /// <summary>
        /// GetBoolAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> GetBoolAsync(Stream stream)
        {
            byte[] lenBuffer = new byte[2];
            await stream.ReadAsync(lenBuffer, 0, 2);
            int length = ByteOrder.Flip(BitConverter.ToInt16(lenBuffer, 0));

            if (length != 1)
            {
                throw new Exception("Invalid boolean Value");
            }

            byte[] boolBuffer = new byte[1];
            await stream.ReadAsync(boolBuffer, 0, 1);
            byte bVal = boolBuffer[0];

            switch (bVal)
            {
                case 0x00:
                    return false;
                case 0x01:
                    return true;
                default:
                    throw new Exception(string.Format($"Boolean value {bVal} not supported."));
            }
        }

        /// <summary>
        /// GetResolutionAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Resolution> GetResolutionAsync(Stream stream)
        {
            byte[] lenBuffer = new byte[2];
            await stream.ReadAsync(lenBuffer, 0, 2);
            int len = ByteOrder.Flip(BitConverter.ToInt16(lenBuffer, 0));

            if (len != 9)
            {
                throw new Exception("Invalid Resolution value length");
            }

            byte[] resolutionBuffer = new byte[len];
            await stream.ReadAsync(resolutionBuffer, 0, len);
            return await Resolution.ParseAsync(resolutionBuffer);
        }


        /// <summary>
        /// Resolution 
        /// </summary>
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

            public static async Task<Resolution> ParseAsync(byte[] byteStream)
            {
                using (MemoryStream ms = new MemoryStream(byteStream))
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        Resolution resolution = new Resolution();
                        resolution.CrossFeedResolution = ByteOrder.Flip(reader.ReadInt32());
                        resolution.FeedResolution = ByteOrder.Flip(reader.ReadInt32());
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
    /// <summary>
    /// JobIdContainer
    /// 
    /// You cannot pass a variable by reference in an async method, to get around this
    /// this class is instantiated by GetIppAttributesAsync so the jobId variable can 
    /// be modified by the called async method. :-)
    /// </summary>
    public class JobIdContainer
    {
        public int JobId { get; set; }
    }

}
