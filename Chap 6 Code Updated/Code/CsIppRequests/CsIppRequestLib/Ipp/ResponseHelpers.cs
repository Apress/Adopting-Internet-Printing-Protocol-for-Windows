using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    /// <summary>
    /// This class is essentially identical to the previous verison of ResponseHelpers, except the stream reads 
    /// have been replaced with SafeStreamRead calls that support cancellation tokens and exact reads. The older version
    /// used stream.ReadAsync which does not guarantee reading the requested number of bytes in one call - this was the 
    /// cause of various bugs when reading IPP responses. We also return a CompletionStruct from GetIppAttributesCollectionAsync 
    /// to better capture job state information from the printer. Note: All methods have been updated to replace stream.ReadAsync
    /// with the (safer) SafeStreamRead methods. The updated CompletionStruct facilitates better job state tracking, especially for
    /// password-protected stored jobs. 
    /// </summary>
    public static class ResponseHelpers
    {
        private const int MaxNameLength = 1024;              // attribute names 
        private const ushort MaxUShort = 65535;             // max ushort value


        /// <summary>
        /// GetIppAttributesCollectionAsync
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <param name="pac"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<CompletionStruct> GetIppAttributesCollectionAsync(Stream stream, IppRequest.RequestType type, IppAttributes pac, CancellationToken ct = default)
        {
            string prevAttribute = null;
            int jobTagIndex = 0;
            var cs = new CompletionStruct();

            while (true)
            {
                byte b = await SafeStreamRead.ReadByteAsync(stream, ct).ConfigureAwait(false);
                var delimiterTag = (DelimiterTag)b;

                switch (delimiterTag)
                {
                    case DelimiterTag.Reserved:
                        break;

                    case DelimiterTag.OperationAttributesTag:
                    case DelimiterTag.PrinterAttributesTag:
                    case DelimiterTag.UnsupportedAttributesTag:
                        // Reset between groups so 1setOf doesn't leak across groups
                        prevAttribute = null;
                        break;

                    case DelimiterTag.JobAttributesTag:
                        jobTagIndex++;
                        prevAttribute = null;
                        break;

                    case DelimiterTag.EndOfAttributesTag:
                        return cs;

                    default:
                        var attribute = await GetAttributeCollectionAsync((ValueTag)b, stream, prevAttribute, type, pac, jobTagIndex, cs, ct).ConfigureAwait(false);

                        prevAttribute = attribute;

                        if (attribute == "EXIT")
                            return cs;

                        break;
                }
            }
        }

        /// <summary>
        /// GetAttributeCollectionAsync
        /// 
        /// Similar to previous version, but now captures job-state and job-state-reasons into CompletionStruct. Additionally, as in 
        /// GetIppAttributesCollectionAsync, the stream read operations have been replaced with SafeStreamRead calls.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="stream"></param>
        /// <param name="previous"></param>
        /// <param name="type"></param>
        /// <param name="pac"></param>
        /// <param name="jobTag"></param>
        /// <param name="cs"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="Exception"></exception>
        private static async Task<string> GetAttributeCollectionAsync(ValueTag tag, Stream stream, string previous, IppRequest.RequestType type, IppAttributes pac, int jobTag, CompletionStruct cs, CancellationToken ct = default)
        {
            // name-length
            ushort nameLen = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (nameLen > MaxNameLength)
                throw new Exception($"Error recovering legitimate attribute name length: {nameLen}");

            // name
            byte[] nameBytes = nameLen == 0 ? Array.Empty<byte>() : new byte[nameLen];
            if (nameLen > 0)
                await SafeStreamRead.ReadExactlyAsync(stream, nameBytes, 0, nameBytes.Length, ct).ConfigureAwait(false);

            string name = nameLen == 0 ? "" : Encoding.UTF8.GetString(nameBytes);

            // 1setOf handling: if name is empty, re-use previous attribute name
            string attributeName = name != string.Empty ? name : previous;
            if (string.IsNullOrEmpty(attributeName))
                throw new Exception("Attribute name not found in a 1setOf");

            // ** Debug **
            //if (tag == ValueTag.BegCollection)
                //Console.WriteLine($"DEBUG: TOP-LEVEL collection attr='{attributeName}'");

            object value = await GetValueTagValueAsync(stream, tag, ct).ConfigureAwait(false);

            // Convert enums to readable string
            if (tag == ValueTag.Enum)
            {
                string mapName = attributeName; // already resolved via 1setOf logic
                if (!string.IsNullOrEmpty(mapName) && Mappings.IsMapped(mapName))
                {
                    if (Mappings.enumMappings.TryGetValue(mapName, out var attributeMappings))
                    {
                        if (attributeMappings.TryGetValue(Convert.ToInt32(value), out var enumStringValue))
                            value = enumStringValue;
                    }
                }
            }

            // New - capture job-state/job-state-reasons if present
            CaptureJobState(attributeName, value, cs);

            if (type == IppRequest.RequestType.GET_PRINTER_ATTRIBUTES)
            {
                IppAttribute pAttr = pac.Find(attributeName);
                if (pAttr == null)
                {
                    IppAttribute pa = new IppAttribute(attributeName, value?.ToString(), (byte)tag);
                    pac.AddAttribute(pa);
                }
                else
                {
                    pAttr.AddAttributeValue(value?.ToString());
                }
            }
            else if (type == IppRequest.RequestType.GET_JOBS)
            {
                IppAttribute pa = new IppAttribute(attributeName, value?.ToString(), (byte)tag, jobTag);
                pac.AddJobAttribute(pa);
            }
            else if (type == IppRequest.RequestType.CREATE_JOB && attributeName.Equals("job-id", StringComparison.OrdinalIgnoreCase))
            {
                cs.jobId = Convert.ToInt32(value);
                return "EXIT";
            }
            else if (type == IppRequest.RequestType.GET_JOB_ATTRIBUTES)
            {
                IppAttribute pa = new IppAttribute(attributeName, value?.ToString(), (byte)tag, jobTag);
                pac.AddJobAttribute(pa);
            }

            return attributeName;
        }


        /// <summary>
        /// CaptureJobState
        /// 
        /// This method was added to provide a better way to capture job state information from the printer. In particular,
        /// this is handly for password protected stored jobs, where the job state may be necessary to determine if the job can
        /// be processed on the printer. In testing, some of the more comlex pdf files were cancelled by the printer after send-document
        /// returned SUCCESS. This should at least provide a heads-up on what happened.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="value"></param>
        /// <param name="cs"></param>
        private static void CaptureJobState(string attributeName, object value, CompletionStruct cs)
        {
            if (cs == null) 
                return;

            if (attributeName.Equals("job-state", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(value?.ToString(), out int js))
                {
                    cs.JobStateEnum = js;

                    // If mapping is available, store friendly name
                    cs.JobStateText = (Mappings.enumMappings.TryGetValue("job-state", out var jsMappings) && jsMappings.TryGetValue(js, out var jsText)) ? jsText : cs.JobStateText;
                }
                else
                {
                    cs.JobStateText = value?.ToString();
                }
            }
            else if (attributeName.Equals("job-state-reasons", StringComparison.OrdinalIgnoreCase))
            {
                var s = value?.ToString();
                if (!string.IsNullOrWhiteSpace(s) && !cs.JobStateReasons.Contains(s))
                {
                    cs.JobStateReasons.Add(s);
                }
            }
        }

        /// <summary>
        ///  RFC8010 Delimiter-Tag values
        /// </summary>
        public enum DelimiterTag : byte
        {
            Reserved = 0x00,
            OperationAttributesTag = 0x01,
            JobAttributesTag = 0x02,
            EndOfAttributesTag = 0x03,
            PrinterAttributesTag = 0x04,
            UnsupportedAttributesTag = 0x05
        }

        /// <summary>
        ///  RFC8010 Value-Tag values
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
            BegCollection = 0x34,                               //3.1.6
            TextWithLanguage = 0x35,
            NameWithLanguage = 0x36,
            EndCollection = 0x37,                               //3.1.6
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
            MemberAttrName = 0x4a,                              //3.1.7
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
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<object> GetValueTagValueAsync(Stream stream, ValueTag vt, CancellationToken ct = default)
        {
            switch (vt)
            {
                case ValueTag.Unsupported:
                case ValueTag.Unknown:
                case ValueTag.NoValue:
                    return await GetNoValueAsync(stream, ct).ConfigureAwait(false);

                case ValueTag.Integer:
                case ValueTag.Enum:
                case ValueTag.IntegerUnassigned20:
                case ValueTag.IntegerUnassigned24:
                case ValueTag.IntegerUnassigned25:
                case ValueTag.IntegerUnassigned26:
                case ValueTag.IntegerUnassigned27:
                case ValueTag.IntegerUnassigned28:
                case ValueTag.IntegerUnassigned29:
                case ValueTag.IntegerUnassigned2A:
                case ValueTag.IntegerUnassigned2B:
                case ValueTag.IntegerUnassigned2C:
                case ValueTag.IntegerUnassigned2D:
                case ValueTag.IntegerUnassigned2E:
                case ValueTag.IntegerUnassigned2F:
                    return await GetIntAsync(stream, ct).ConfigureAwait(false);

                case ValueTag.Boolean:
                    return await GetBoolAsync(stream, ct).ConfigureAwait(false);

                case ValueTag.DateTime:
                    return await GetDateTimeOffsetAsync(stream, ct).ConfigureAwait(false);

                case ValueTag.Resolution:
                    return await GetResolutionAsync(stream, ct).ConfigureAwait(false);

                case ValueTag.RangeOfInteger:
                    return await GetRangeAsync(stream, ct).ConfigureAwait(false);

                case ValueTag.TextWithLanguage:
                case ValueTag.NameWithLanguage:
                    return await GetStringWithLanguageAsync(stream, ct).ConfigureAwait(false);

                case ValueTag.BegCollection:
                    //Update handling of returned collection streams
                    return await ReadCollectionStringAsync(stream, "", ct).ConfigureAwait(false);

                case ValueTag.EndCollection:
                case ValueTag.MemberAttrName:
                    throw new Exception($"{vt} is only valid inside a collection...");

                default:
                    return await GetStringAsync(stream, ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// GetDateTimeOffsetAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<DateTimeOffset> GetDateTimeOffsetAsync(Stream stream, CancellationToken ct = default)
        {
            ushort len = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (len != 11)
            {
                throw new Exception("Invalid DateTime attribute length");
            }

            byte[] dtArray = new byte[11];
            await SafeStreamRead.ReadExactlyAsync(stream, dtArray, 0, 11, ct).ConfigureAwait(false);

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
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<StringWithLanguage> GetStringWithLanguageAsync(Stream stream, CancellationToken ct = default)
        {
            // Outer IPP value-length (covers the entire composite payload)
            ushort valueLen = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (valueLen == 0)
            {
                return new StringWithLanguage("", "");
            }

            // Read the composite payload
            byte[] payload = await SafeStreamRead.ReadBytesAsync(stream, valueLen, ct).ConfigureAwait(false);

            // Parse: [2 bytes langLen][lang][2 bytes strLen][str]
            int p = 0;
            if (payload.Length < 4)
            {
                return new StringWithLanguage("", Encoding.UTF8.GetString(payload));
            }

            ushort langLen = (ushort)((payload[p] << 8) | payload[p + 1]); p += 2;
            if (p + langLen + 2 > payload.Length)
            {
                return new StringWithLanguage("", BitConverter.ToString(payload));
            }

            string lang = langLen == 0 ? "" : Encoding.ASCII.GetString(payload, p, langLen);
            p += langLen;

            ushort strLen = (ushort)((payload[p] << 8) | payload[p + 1]); p += 2;
            if (p + strLen > payload.Length)
            {
                return new StringWithLanguage(lang, BitConverter.ToString(payload));
            }

            string val = strLen == 0 ? "" : Encoding.UTF8.GetString(payload, p, strLen);
            return new StringWithLanguage(lang, val);
        }




        /// <summary>
        /// GetIntAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<int> GetIntAsync(Stream stream, CancellationToken ct = default)
        {
            ushort length = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (length != 4)
            {
                throw new Exception("Invalid Integer value");
            }

            return await SafeStreamRead.ReadInt32Async(stream, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// GetNoValueAsync
        /// Ref: Use of unknown and no-value attribute value tag
        /// Ref: https://www.pwg.org/archives/ipp/2011/016909.html
        /// Semantically-speaking "unknown" and "no-value" do mean different things:
        /// 1. unknown: There is a value but we don't know what it is.
        /// 2. no-value: We know there is no value.

        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<string> GetNoValueAsync(Stream stream, CancellationToken ct = default)
        {
            ushort length = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
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
            public override string ToString() => $"{Value}[{Language}]";
        }

        /// <summary>
        /// GetRangeAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Range> GetRangeAsync(Stream stream, CancellationToken ct = default)
        {
            ushort length = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (length != 8)
            {
                throw new Exception("Invalid Range value");
            }

            byte[] rangeBuffer = new byte[length];
            await SafeStreamRead.ReadExactlyAsync(stream, rangeBuffer, 0, rangeBuffer.Length, ct).ConfigureAwait(false);
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
                {
                    throw new ArgumentException("Range byte stream must be 8 bytes.");
                }

                var r = new Range
                {
                    Start = (byteStream[0] << 24) | (byteStream[1] << 16) | (byteStream[2] << 8) | byteStream[3],
                    End = (byteStream[4] << 24) | (byteStream[5] << 16) | (byteStream[6] << 8) | byteStream[7],
                };
                return r;
            }

            public override string ToString() => $"Range: {Start} - {End}";
        }

        /// <summary>
        ///  GetStringAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public static async Task<string> GetStringAsync(Stream stream, CancellationToken ct = default)
        {
            ushort len = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (len > MaxUShort)
            {
                throw new InvalidDataException($"Unreasonable string length: {len}");
            }

            if (len == 0)
            {
                return string.Empty;
            }

            byte[] buf = new byte[len];
            await SafeStreamRead.ReadExactlyAsync(stream, buf, 0, buf.Length, ct).ConfigureAwait(false);
            return Encoding.UTF8.GetString(buf);
        }

        /// <summary>
        /// GetBoolAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> GetBoolAsync(Stream stream, CancellationToken ct = default)
        {
            ushort length = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (length != 1)
            {
                throw new Exception("Invalid boolean Value");
            }

            byte bVal = await SafeStreamRead.ReadByteAsync(stream, ct).ConfigureAwait(false);

            switch (bVal)
            {
                case 0x00:
                    return false;
                case 0x01:
                    return true;
                default:
                    throw new Exception($"Boolean value {bVal} is invalid...");
            }

        }

        /// <summary>
        /// GetResolutionAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Resolution> GetResolutionAsync(Stream stream, CancellationToken ct = default)
        {
            ushort len = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (len != 9)
            {
                throw new Exception("Invalid Resolution value length");
            }

            byte[] resolutionBuffer = new byte[len];
            await SafeStreamRead.ReadExactlyAsync(stream, resolutionBuffer, 0, resolutionBuffer.Length, ct).ConfigureAwait(false);
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
                {
                    throw new ArgumentException("Resolution byte stream must be 9 bytes.");
                }

                return new Resolution
                {
                    CrossFeedResolution = (byteStream[0] << 24) | (byteStream[1] << 16) | (byteStream[2] << 8) | byteStream[3],
                    FeedResolution = (byteStream[4] << 24) | (byteStream[5] << 16) | (byteStream[6] << 8) | byteStream[7],
                    Unit = (ResolutionUnit)byteStream[8]
                };
            }

            public override string ToString() => $"Resolution: {CrossFeedResolution} x {FeedResolution} {Unit}";
        }

        //The methods below are used to read and render collection values from the IPP response into a suitable format beginning with '{' and ending with '}'.  

        /// <summary>
        ///  ReadCollectionStringAsync
        ///  
        /// Gets called recursively for nested collections. See addendum 3 of the book for the structure of collections.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="collectionNameForPrefix"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        private static async Task<string> ReadCollectionStringAsync(Stream stream, string collectionNameForPrefix, CancellationToken ct = default)
        {
            // begCollection: consume its valueLen (normally 0)
            ushort valueLen = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (valueLen != 0)
            {
                byte[] vlb = await SafeStreamRead.ReadBytesAsync(stream, valueLen, ct).ConfigureAwait(false);
            }

            bool isTopLevelValue = string.IsNullOrEmpty(collectionNameForPrefix);

            // Build a string of members + nested collections - pass this back to the caller
            var content = new StringBuilder();

            // For nested collections, prefix with the member name (i.e. "media-size")
            if (!isTopLevelValue)
            {
                content.Append(collectionNameForPrefix);
            }

            while (true)
            {
                byte tagByte = await SafeStreamRead.ReadByteAsync(stream, ct).ConfigureAwait(false);
                var tag = (ValueTag)tagByte;

                if (tag == ValueTag.EndCollection)
                {
                    // endCollection: nameLen=0, valueLen=0 - both are consumed and not used: Debug check
                    string ns = await ReadIppNameAsync(stream, ct).ConfigureAwait(false);
                    byte[] vb = await ReadIppValueBytesAsync(stream, ct).ConfigureAwait(false);
                    break;
                }

                if (tag != ValueTag.MemberAttrName)
                {
                    throw new InvalidDataException($"Expected member-attr-name (0x4A) inside collection, got 0x{tagByte:X2}. (prefix={collectionNameForPrefix})");
                }

                // member-attr-name: nameLen=0, valueBytes are the member name
                string ippn = await ReadIppNameAsync(stream, ct).ConfigureAwait(false); // should be ""
                byte[] memberNameBytes = await ReadIppValueBytesAsync(stream, ct).ConfigureAwait(false);
                string memberName = Encoding.UTF8.GetString(memberNameBytes);

                // Now read 1+ values for this member until next MemberAttrName or EndCollection
                while (true)
                {
                    byte nextTagByte = await SafeStreamRead.ReadByteAsync(stream, ct).ConfigureAwait(false);

                    if (nextTagByte == (byte)ValueTag.MemberAttrName || nextTagByte == (byte)ValueTag.EndCollection)
                    {
                        // Detect boundary of collection by reading ahead one byte to find the next member-attr-name (0x4A) or the
                        // end -collection (0x37) tag. If found, push it back for the outer loop to process.
                        SafeStreamRead.PushbackByte(stream, nextTagByte);
                        break;
                    }

                    var memberValueTag = (ValueTag)nextTagByte;

                    // Member values always have nameLen=0
                    string mvn = await ReadIppNameAsync(stream, ct).ConfigureAwait(false); // should be ""

                    if (memberValueTag == ValueTag.BegCollection)
                    {
                        // Nested collection: ReadCollectionStringAsync will consume its own valueLen, memberName is true
                        string nested = await ReadCollectionStringAsync(stream, memberName, ct).ConfigureAwait(false);
                        content.Append('{').Append(nested).Append('}');
                        continue;
                    }

                    byte[] memberValueBytes = await ReadIppValueBytesAsync(stream, ct).ConfigureAwait(false);
                    string renderedValue = RenderSimpleValue(memberValueTag, memberValueBytes);

                    content.Append('{').Append(memberName).Append('=').Append(renderedValue).Append('}');
                }
            }

            // Wrap top level of collection values in braces 
            if (isTopLevelValue)
            {
                return "{" + content.ToString() + "}";
            }

            return content.ToString();
        }

       
        /// <summary>
        /// ReadIppNameAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private static async Task<string> ReadIppNameAsync(Stream stream, CancellationToken ct)
        {
            ushort nameLen = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (nameLen == 0)
            {
                return string.Empty;
            }

            byte[] nameBytes = await SafeStreamRead.ReadBytesAsync(stream, nameLen, ct).ConfigureAwait(false);
            return Encoding.UTF8.GetString(nameBytes);
        }

        /// <summary>
        /// ReadIppValueBytesAsync
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private static async Task<byte[]> ReadIppValueBytesAsync(Stream stream, CancellationToken ct)
        {
            ushort valueLen = await SafeStreamRead.ReadUInt16Async(stream, ct).ConfigureAwait(false);
            if (valueLen == 0)
            {
                return Array.Empty<byte>();
            }

            return await SafeStreamRead.ReadBytesAsync(stream, valueLen, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// RenderSimpleValue
        /// 
        /// Return human readable values for value tags
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="valueBytes"></param>
        /// <returns></returns>
        private static string RenderSimpleValue(ValueTag tag, byte[] valueBytes)
        {
            switch (tag)
            {
                case ValueTag.Integer:
                case ValueTag.Enum:
                    // Normal: 4 bytes
                    if (valueBytes.Length == 4)
                    {
                        int i = (valueBytes[0] << 24) | (valueBytes[1] << 16) | (valueBytes[2] << 8) | valueBytes[3];
                        return i.ToString();
                    }

                    if (valueBytes.Length == 8)
                    {
                        int lo = (valueBytes[0] << 24) | (valueBytes[1] << 16) | (valueBytes[2] << 8) | valueBytes[3];
                        int hi = (valueBytes[4] << 24) | (valueBytes[5] << 16) | (valueBytes[6] << 8) | valueBytes[7];
                        return $"{lo}-{hi}";
                    }

                    return BitConverter.ToString(valueBytes);

                case ValueTag.RangeOfInteger: // 0x33
                    if (valueBytes.Length == 8)
                    {
                        int lo = (valueBytes[0] << 24) | (valueBytes[1] << 16) | (valueBytes[2] << 8) | valueBytes[3];
                        int hi = (valueBytes[4] << 24) | (valueBytes[5] << 16) | (valueBytes[6] << 8) | valueBytes[7];
                        return $"{lo}-{hi}";
                    }
                    return BitConverter.ToString(valueBytes);

                case ValueTag.Boolean:
                    if (valueBytes.Length != 1)
                    {
                        return BitConverter.ToString(valueBytes);
                    }
                    return valueBytes[0] == 0x01 ? "true" : "false";

                case ValueTag.TextWithLanguage:
                case ValueTag.NameWithLanguage:
                    return RenderStringWithLanguage(valueBytes);

                case ValueTag.TextWithoutLanguage:
                case ValueTag.NameWithoutLanguage:
                case ValueTag.Keyword:
                case ValueTag.Uri:
                case ValueTag.UriScheme:
                case ValueTag.Charset:
                case ValueTag.NaturalLanguage:
                case ValueTag.MimeMediaType:
                case ValueTag.OctetStringWithAnUnspecifiedFormat:
                    return Encoding.UTF8.GetString(valueBytes);

                default:
                    return BitConverter.ToString(valueBytes);
            }
        }

        /// <summary>
        /// RenderStringWithLanguage
        /// </summary>
        /// <param name="valueBytes"></param>
        /// <returns></returns>
        private static string RenderStringWithLanguage(byte[] valueBytes)
        {
            // valueBytes = [2 bytes langLen][lang][2 bytes textLen][text]
            if (valueBytes == null || valueBytes.Length < 4)
            {
                return BitConverter.ToString(valueBytes);
            }

            int p = 0;
            int langLen = (valueBytes[p] << 8) | valueBytes[p + 1]; p += 2;
            if (p + langLen + 2 > valueBytes.Length)
            {
                return BitConverter.ToString(valueBytes);
            }

            string lang = langLen == 0 ? "" : Encoding.ASCII.GetString(valueBytes, p, langLen);
            p += langLen;

            int textLen = (valueBytes[p] << 8) | valueBytes[p + 1]; p += 2;
            if (p + textLen > valueBytes.Length)
            {
                return BitConverter.ToString(valueBytes);
            }

            string text = textLen == 0 ? "" : Encoding.UTF8.GetString(valueBytes, p, textLen);

            return string.IsNullOrEmpty(lang) ? text : $"{text}[{lang}]";
        }
    }
}
