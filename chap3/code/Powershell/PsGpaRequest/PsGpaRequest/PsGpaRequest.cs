using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace PsGpaRequest
{
    [Cmdlet(VerbsCommon.Get, "PrinterAttributes")]
    public class PsGpaRequest : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "The name of the printer to get attributes from")]
        public string PrinterName { get; set; }

        [Parameter(Position = 1, Mandatory = true, HelpMessage = "requested attributes, separated by commas")]
        public string AttributesRequired { get; set; }
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            WriteObject("Starting...");
            string[] req_attributes = AttributesRequired.Split(',');
            MakeRequest(PrinterName, req_attributes);
        }

        [Cmdlet(VerbsCommon.Get, "HelpMessage")]
        public class GetHelpMessageCommand : Cmdlet
        {
            [Parameter(Mandatory = true, Position = 0, HelpMessage = "Enter the topic you need help with.")]
            public string Topic { get; set; }

            protected override void ProcessRecord()
            {
                string helpMessage = GetHelpMessage(Topic);
                WriteObject(helpMessage);
            }

            private string GetHelpMessage(string topic)
            {
                // In a real-world scenario, you might query a help database or API.
                // For this example, we'll use a simple switch statement.
                switch (topic.ToLower())
                {
                    case "usage":
                        return "Get-PrinterAttributes -PrinterName \"<printer-name\" -AttributesRequired \"<attributes-required\"";
                    case "example":
                        return "Get-PrinterAttributes -PrinterName \"Printer1\" -AttributesRequired \"printer-description\"";
                    default:
                        return "Help topic not found. Please provide a valid topic.";
                }
            }
        }

        public void MakeRequest(string printer_name, string[] requested_attributes)
        {
            var byteList = new List<byte>();

            if ((printer_name == null) || (requested_attributes.Length == 0))
            {
                Console.WriteLine("Error - a printer name and requested attributes are required");
                return;
            }

            var printerUri = "http://" + printer_name + "/ipp/print";
            var printerUriBytes = Encoding.UTF8.GetBytes(printerUri);

            // Define the IPP request components
            var ippVersionAndOperationId = new byte[] { 0x01, 0x01, 0x00, 0x0B }; // IPP version and operation ID
            var requestId = new byte[] { 0x00, 0x00, 0x00, 0x01 }; // Request ID
            var operationAttributesTag = new byte[] { 0x01 }; // Operation attributes tag

            // Create attributes with dynamic length calculation
            var attributesCharset = RequestHelpers.CreateAttribute(0x47, "attributes-charset", "utf-8", false);
            var attributesNaturalLanguage = RequestHelpers.CreateAttribute(0x48, "attributes-natural-language", "en", false);
            var printerUriAttribute = RequestHelpers.CreateAttribute(0x45, "printer-uri", printerUri, false);

            for (int i = 0; i < requested_attributes.Length; i++)
            {
                if (i == 0)
                    byteList.AddRange(RequestHelpers.CreateAttribute(0x44, "requested-attributes", requested_attributes[i], false));
                else
                    byteList.AddRange(RequestHelpers.CreateAttribute(0x44, "requested-attributes", requested_attributes[i], true));
            }
            byte[] concatenatedArray = byteList.ToArray();
            var endOfAttributesTag = new byte[] { 0x03 }; // End of attributes tag

            // Combine all components into the final request payload
            var requestPayload = ippVersionAndOperationId
                .Concat(requestId)
                .Concat(operationAttributesTag)
                // Add the attributes
                .Concat(attributesCharset)
                .Concat(attributesNaturalLanguage)
                .Concat(printerUriAttribute)
                // Add the requested-attributes
                .Concat(concatenatedArray)
                .Concat(endOfAttributesTag)
                .ToArray();

            // Set timeout default at 30 seconds. For printers sleeping, you might have to adjust this...
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                var content = new ByteArrayContent(requestPayload);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/ipp");

                try
                {
                    var response = client.PostAsync(printerUri, content).GetAwaiter().GetResult();
                    WriteObject("Get-Printer-Attributes Response:");
                    GetIppResponse(response);
                }
                catch (TaskCanceledException)
                {
                    WriteObject("Request timed out.");
                }
                catch (Exception ex)
                {
                    WriteObject($"An error occurred: {ex.Message}");
                }
            }
        }


        /// <summary>
        /// GetIppResponse
        /// 
        /// Read and print out the response from the printer.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        void GetIppResponse(HttpResponseMessage response)
        {
            try
            {
                using (var responseStream = response.Content.ReadAsStreamAsync().Result)
                {
                    using (var reader = new BinaryReader(responseStream, Encoding.ASCII, true))
                    {
                        if (responseStream.Length == 0)
                        {
                            throw new Exception(string.Format("Invalid IPP response stream from printer, length was 0"));
                        }
                        //Look at the first 2 bytes
                        short Version = ByteOrder.Flip(reader.ReadInt16());
                        var MajorByte = (byte)(Version >> 8 & 0xFF);
                        var MinorByte = (byte)(Version & 0xFF);
                        WriteObject($"The version returned was {MajorByte}.{MinorByte}");
                        //Look at the next 2 bytes
                        short StatusCode = ByteOrder.Flip(reader.ReadInt16());
                        if (StatusCode > 0xff)
                        {
                            /*============================================================================================================
                            RFC 8011              IPP / 1.1: Model and Semantics January 2017
                            The status-code values range from 0x0000 to 0x7fff.The value rangesfor each status-code class are as follows:
                            "successful" - 0x0000 to 0x00ff
                            =============================================================================================================*/
                            throw new Exception(string.Format("Ipp response error, status code was: {0}", StatusCode));
                        }
                        WriteObject($"The Status Code was: {StatusCode}");                        //Look at the next 4 bytes
                        int RequestId = ByteOrder.Flip(reader.ReadInt32());
                        WriteObject($"The request Id was: {RequestId}");
                        //Get the rest of the IPP response
                        GetIppAttributes(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("HttpResponseMessage exception thrown - error reading response, reason: {0}", ex.Message));
            }

        }

        public void GetIppAttributes(BinaryReader reader)
        {
            string prevAttribute = null;

            do
            {
                var b = reader.ReadByte();
                var delimiterTag = (DelimiterTag)b;

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
        public string GetAttribute(ValueTag tag, BinaryReader stream, string previous)
        {
            var len = ByteOrder.Flip(stream.ReadInt16());
            var name = Encoding.ASCII.GetString(stream.ReadBytes(len));
            var value = GetValueTagValue(stream, tag);
            //If the name is null, then this is a 1setOf 
            var attName = string.IsNullOrEmpty(name) && previous != null ? previous : name;

            if (string.IsNullOrEmpty(attName))
            {
                throw new Exception("Attribute name not found in a 1setOf");
            }

            //Write out the attribute name and value

            WriteObject(string.Format("Attribute Name: {0}, Attribute Value: {1}", name != string.Empty ? name : previous, value));

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
        /// GetValueTagValue
        /// 
        /// </summary>
        /// <param name="stream">The BinaryReader object that holds the byte stream</param>
        /// <param name="vt">The value tag</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object GetValueTagValue(BinaryReader stream, ValueTag vt)
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
                    WriteObject("{");
                    return GetString(stream);
                case ValueTag.TextWithLanguage:
                    return GetStringWithLanguage(stream);
                case ValueTag.NameWithLanguage:
                    return GetStringWithLanguage(stream);
                case ValueTag.EndCollection:
                    WriteObject("}");
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
                case ValueTag.OctetStringUnassigned38:
                case ValueTag.OctetStringUnassigned39:
                case ValueTag.OctetStringUnassigned3A:
                case ValueTag.OctetStringUnassigned3B:
                case ValueTag.OctetStringUnassigned3C:
                case ValueTag.OctetStringUnassigned3D:
                case ValueTag.OctetStringUnassigned3E:
                case ValueTag.OctetStringUnassigned3F:
                    return "Value Tag: OctetStringUnassignednn";
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
                case ValueTag.StringUnassigned40:
                case ValueTag.StringUnassigned43:
                case ValueTag.StringUnassigned4B:
                case ValueTag.StringUnassigned4C:
                case ValueTag.StringUnassigned4D:
                case ValueTag.StringUnassigned4E:
                case ValueTag.StringUnassigned4F:
                case ValueTag.StringUnassigned50:
                case ValueTag.StringUnassigned51:
                case ValueTag.StringUnassigned52:
                case ValueTag.StringUnassigned53:
                case ValueTag.StringUnassigned54:
                case ValueTag.StringUnassigned55:
                case ValueTag.StringUnassigned56:
                case ValueTag.StringUnassigned57:
                case ValueTag.StringUnassigned58:
                case ValueTag.StringUnassigned59:
                case ValueTag.StringUnassigned5A:
                case ValueTag.StringUnassigned5B:
                case ValueTag.StringUnassigned5C:
                case ValueTag.StringUnassigned5D:
                case ValueTag.StringUnassigned5E:
                case ValueTag.StringUnassigned5F:
                    return "Value Tag: StringUnassignednn";
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

            var dateTimeOffset = new DateTimeOffset(year, month, day, hour, minute, second, deciSecond * 100, new TimeSpan(0, 0, 0));
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

