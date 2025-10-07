using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public static class AttributeHelper
    {
        public static byte GetJobAttributeByte(string job_attribute)
        {
            string _jal = job_attribute.ToLower();

            if (_jal == "media")
                return (byte)ValueTag.Keyword;
            if (_jal == "copies")
                return (byte)ValueTag.Integer;
            if (_jal == "sides")
                return (byte)ValueTag.Keyword;
            if (_jal == "media-col")
                return (byte)ValueTag.BegCollection;
            if (_jal == "cover-col")
                return (byte)ValueTag.BegCollection;
            if (_jal == "job-sheets-col")
                return (byte)ValueTag.BegCollection;
            if (_jal == "finishing-template-col")
                return (byte)ValueTag.BegCollection;
            if (_jal == "print-quality")
                return (byte)ValueTag.Enum;
            if (_jal == "print-color-mode")
                return (byte)ValueTag.Keyword;
            if (_jal == "print-scaling")
                return (byte)ValueTag.Keyword;
            if (_jal == "printer-resolution")
                return (byte)ValueTag.Resolution;
            if (_jal == "page-ranges")
                return (byte)ValueTag.RangeOfInteger;
            if (_jal == "finishings")
                return (byte)ValueTag.Enum;
            if (_jal == "output-bin")
                return (byte)ValueTag.Keyword;
            if (_jal == "orientation-requested")
                return (byte)ValueTag.Enum;
            if (_jal == "job-name")
                return (byte)ValueTag.Keyword;
            if (_jal == "multiple-document-handling")
                return (byte)ValueTag.Keyword;
            if (_jal == "printer-resolution")
                return (byte)ValueTag.Resolution;
            else
                throw new Exception($"Unsupported job attribute: {job_attribute}");
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
        /// ProcessFile
        /// 
        /// Split the job attributes file into lines, each specifying a job attribute..
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Dictionary<string, List<object>> ProcessFile(string fileName)
        {
            var jobAttributes = new Dictionary<string, List<object>>();
            foreach (var line in File.ReadLines(fileName))
            {
                ProcessLine(line, jobAttributes);
            }

            return jobAttributes;
        }

        private static void ProcessLine(string line, Dictionary<string, List<object>> jobAttributes)
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();

                    if (!jobAttributes.ContainsKey(key))
                    {
                        jobAttributes[key] = new List<object>();
                    }

                    //parse the value and add to the dictionary
                    var parsedValue = ParseValue(value);
                    jobAttributes[key].Add(parsedValue);
                }
                else
                {
                    int pos = part.IndexOf('=');
                    if (pos != -1)
                    {
                        string key = part.Substring(0, pos).Trim();
                        string value = part.Substring(pos + 1).Trim();
                        if (IsCollection(value) == true)
                        {
                            if (!jobAttributes.ContainsKey(key))
                            {
                                jobAttributes[key] = new List<object>();
                                jobAttributes[key].Add(value);
                            }
                           
                        }
                    }
                }
            }
        }

        private static object ParseValue(string value)
        {
            if (value.Contains(","))
            {
                //if the value contains a comma it is a list of values
                var items = value.Split(',');
                List<object> list = new List<object>();

                foreach (var item in items)
                {
                    //recursively add items
                    list.Add(ParseValue(item.Trim()));
                }
                return list;
            }
            else if (int.TryParse(value, out int intValue))
            {
                return intValue;
            }
            else
            {
                //keep value as string
                return value;
            }
        }

        /// <summary>
        ///  IsCollection
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
    }
}

  