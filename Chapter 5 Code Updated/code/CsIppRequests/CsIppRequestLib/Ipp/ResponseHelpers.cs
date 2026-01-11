using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace CsIppRequestLib
{
    public static class ResponseHelpers
    {
        private const int MaxNameLength = 1024;              // attribute names 
        private const int MaxValueLength = 4 * 1024 * 1024;  // 4MB value blob
        private const ushort MaxUShort = 65535;             // max ushort value

        /// <summary>
        /// GetIppAttributesAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<int> GetIppAttributesAsync(Stream stream)
        {
            string prevAttribute = null;
            var jobIdContainer = new JobIdContainer { JobId = -1 };

            do
            {
                byte b = await SafeStreamRead.ReadByteAsync(stream).ConfigureAwait(false);

                if (stream.Position == stream.Length)
                    return jobIdContainer.JobId;

                //await stream.ReadAsync(buffer, 0, 1);
                DelimiterTag delimiterTag = (DelimiterTag)b;

                switch (delimiterTag)
                {
                    case DelimiterTag.Reserved:
                        break;
                    case DelimiterTag.OperationAttributesTag:
                        break;
                    case DelimiterTag.JobAttributesTag:
                        break;
                    case DelimiterTag.PrinterAttributesTag:
                        break;
                    case DelimiterTag.UnsupportedAttributesTag:
                        break;
                    case DelimiterTag.EndOfAttributesTag:
                        return -1;
                    default:
                        var attribute = await GetAttributeAsync((ValueTag)delimiterTag, stream, prevAttribute, jobIdContainer);
                        prevAttribute = attribute;

                        if (prevAttribute == null && attribute == null)
                        {
                            throw new Exception("Invalid IPP response stream!");
                        }

                        if (attribute == "EXIT")
                        {
                            return jobIdContainer.JobId;
                        }
                        break;
                }
            }
            while (true);
        }

        /// <summary>
        /// GetAttributeAsync
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="stream"></param>
        /// <param name="previous"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static async Task<string> GetAttributeAsync(ValueTag tag, Stream stream, string previous, JobIdContainer jobIdContainer)
        {
            ushort len = await SafeStreamRead.ReadUInt16Async(stream).ConfigureAwait(false);
            if (len > MaxNameLength)
                throw new Exception($"Error recovering legitimate attribute name length: {len}");

            byte[] nameBuffer = new byte[len];
            await stream.ReadAsync(nameBuffer, 0, len);
            string name = Encoding.UTF8.GetString(nameBuffer);

            var value = await GetValueTagValueAsync(stream, tag);

            string attName = string.IsNullOrEmpty(name) && previous != null ? previous : name;

            if (string.IsNullOrEmpty(attName))
            {
                throw new Exception("Attribute name not found in a 1setOf");
            }

            if (name.ToLower() == "job-id")
            {
                jobIdContainer.JobId = Convert.ToInt32(value);
                return "EXIT";
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
            };
        }


        /// <summary>
        /// GetDateTimeOffsetAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<DateTimeOffset> GetDateTimeOffsetAsync(Stream stream)
        {
            ushort len = await SafeStreamRead.ReadUInt16Async(stream).ConfigureAwait(false);
            if (len != 11)
                throw new Exception("Invalid DateTime attribute length");

            byte[] dtArray = new byte[11];
            await SafeStreamRead.ReadExactlyAsync(stream, dtArray, 0, 11).ConfigureAwait(false);

            int year = (dtArray[0] << 8) | dtArray[1];
            int month = dtArray[2];
            int day = dtArray[3];
            int hour = dtArray[4];
            int minute = dtArray[5];
            int second = dtArray[6];
            int deciSecond = dtArray[7];

            return new DateTimeOffset(year, month, day, hour, minute, second, deciSecond * 100, TimeSpan.Zero);
        }


        /// <summary>
        /// GetStringWithLanguageAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<StringWithLanguage> GetStringWithLanguageAsync(Stream stream)
        {
            // Per RFC, this is the language-length
            int bytesRead = await SafeStreamRead.ReadUInt16Async(stream).ConfigureAwait(false);
            var language = await GetStringAsync(stream).ConfigureAwait(false);
            var value = await GetStringAsync(stream).ConfigureAwait(false);
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
            ushort length = await SafeStreamRead.ReadUInt16Async(stream).ConfigureAwait(false);
            if (length != 4)
                throw new Exception("Invalid Integer value");

            return await SafeStreamRead.ReadInt32Async(stream).ConfigureAwait(false);
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
            ushort length = await SafeStreamRead.ReadUInt16Async(stream).ConfigureAwait(false);
            if (length != 0)
                throw new Exception("Invalid NoValue value");

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

        /// <summary>
        /// GetRangeAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception> 
        public static async Task<Range> GetRangeAsync(Stream stream)
        {
            ushort length = await SafeStreamRead.ReadUInt16Async(stream).ConfigureAwait(false);
            if (length != 8)
                throw new Exception("Invalid Range value");

            byte[] rangeBuffer = new byte[length];
            await SafeStreamRead.ReadExactlyAsync(stream, rangeBuffer, 0, rangeBuffer.Length).ConfigureAwait(false);
            return Range.Parse(rangeBuffer);
        }

        /// <summary>
        /// Range
        /// </summary>
        public struct Range
        {
            public int Start { get; private set; }
            public int End { get; private set; }

            public static Range Parse(byte[] byteStream)
            {
                if (byteStream == null || byteStream.Length != 8)
                    throw new ArgumentException("Range byte stream must be 8 bytes.");

                Range r = new Range
                {
                    Start = (byteStream[0] << 24) | (byteStream[1] << 16) | (byteStream[2] << 8) | byteStream[3],
                    End = (byteStream[4] << 24) | (byteStream[5] << 16) | (byteStream[6] << 8) | byteStream[7],
                };
                return r;
            }
            //Put in HR format string
            public override string ToString() => $"Range: {Start} - {End}";
        }

        /// <summary>
        /// GetStringAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<string> GetStringAsync(Stream stream)
        {
            ushort len = await SafeStreamRead.ReadUInt16Async(stream).ConfigureAwait(false);
            if (len > MaxUShort)
                throw new InvalidDataException($"Unreasonable string length: {len}");

            if (len == 0) return string.Empty;

            byte[] buf = new byte[len];
            await SafeStreamRead.ReadExactlyAsync(stream, buf, 0, buf.Length).ConfigureAwait(false);
            return Encoding.UTF8.GetString(buf);

        }

        /// <summary>
        /// GetBoolAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> GetBoolAsync(Stream stream)
        {
            ushort length = await SafeStreamRead.ReadUInt16Async(stream).ConfigureAwait(false);
            if (length != 1)
                throw new Exception("Invalid boolean Value");

            byte bVal = await SafeStreamRead.ReadByteAsync(stream).ConfigureAwait(false);

            switch (bVal)
            {
                case 0x00:
                    return false;
                case 0x01:
                    return true;
                default:
                    throw new Exception($"Boolean value {bVal} not supported.");
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
            ushort len = await SafeStreamRead.ReadUInt16Async(stream).ConfigureAwait(false);
            if (len != 9)
                throw new Exception("Invalid Resolution value length");

            byte[] resolutionBuffer = new byte[len];
            await SafeStreamRead.ReadExactlyAsync(stream, resolutionBuffer, 0, resolutionBuffer.Length).ConfigureAwait(false);
            return Resolution.Parse(resolutionBuffer);
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

            public static Resolution Parse(byte[] byteStream)
            {
                if (byteStream == null || byteStream.Length != 9)
                    throw new ArgumentException("Resolution byte stream must be 9 bytes.");

                return new Resolution
                {
                    CrossFeedResolution = (byteStream[0] << 24) | (byteStream[1] << 16) | (byteStream[2] << 8) | byteStream[3],
                    FeedResolution = (byteStream[4] << 24) | (byteStream[5] << 16) | (byteStream[6] << 8) | byteStream[7],
                    Unit = (ResolutionUnit)byteStream[8]
                };
            }

            //Make HR format string 
            public override string ToString() => $"Resolution: {CrossFeedResolution} x {FeedResolution} {Unit}";
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
