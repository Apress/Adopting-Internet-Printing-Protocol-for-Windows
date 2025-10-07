using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Printers;
using Windows.Devices.Printers.Extensions;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace WinRtIppTest
{
    internal class Program
    {
        private static string printer_name;
        private static string requested_string;
        static void Main(string[] args)
        {
            IppPrintDevice printer = null;
            List<string> requested_attributes = new List<string>();

            try
            {
                GetCommandLineArgs(args);
            }
            catch (Exception ex)
            {
                ShowUsage();
                Console.WriteLine(ex.ToString());
                return;
            }

            try
            {
                requested_attributes = requested_string.Split(',').ToList<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error, could not recover requested attributes, reason: " + ex.Message);
                return;
            }

            try
            {
                printer = IppPrintDevice.FromPrinterName(printer_name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error - unable to locate {printer_name} - ensure it is installed as an IPP printer. Reason for error: {ex.Message}");
                return;
            }
            if (printer != null)
            {
                var attributes = printer.GetPrinterAttributes(requested_attributes);
                DisplayPrinterAttributes(attributes);
            }
            else
            {
                Console.WriteLine("Printer not found.");
            }
            Console.WriteLine("fini");
        }

        /// <summary>
        /// GetCommandLineArgs
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        private static void GetCommandLineArgs(string[] args)
        {
            int count = 0;
            if (args.Length != 2)
            {
                throw new Exception("Invalid command line arguments count!");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("/p=", StringComparison.CurrentCultureIgnoreCase))
                    {
                        printer_name = args[i].Substring(3);
                        count++;
                    }
                    else if (args[i].StartsWith("/r=", StringComparison.CurrentCultureIgnoreCase))
                    {
                        requested_string = args[i].Substring(3);
                        count++;
                    }
                }
                if (count != 2)
                {
                    throw new Exception("Processing of command line failed, invalid arguments");
                }
                else
                {
                    return;
                }
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage: WinRtIppTest /p=<printer_name> /r=<comma delimited requested attributes>");
            Console.WriteLine("Where <printer_name> is the name of the target printer");
            Console.WriteLine("Where <comma delimited requested attributes> are requested attributes separated by comma");
            Console.WriteLine("Example: GetPrinterAttributesRequest /p=printer1 /r=printer-uri-supported,ipp-versions-supported,all");
            return;
        }

        public static void DisplayPrinterAttributes(IDictionary<string, IppAttributeValue> attributes)
        {
            try
            {
                foreach (var attribute in attributes)
                {
                    Console.WriteLine($"{attribute.Key.ToString()}:");
                    var IppValueType = attribute.Value.Kind;
                    switch (IppValueType)
                    {
                        case IppAttributeValueKind.OctetString:
                            var octetStringValues = attribute.Value.GetOctetStringArray();
                            foreach (var buffer in octetStringValues)
                            {
                                using (var dataReader = DataReader.FromBuffer(buffer))
                                {
                                    byte[] byteArray = new byte[buffer.Length];
                                    dataReader.ReadBytes(byteArray);
                                    Console.WriteLine($"Value: {Encoding.UTF8.GetString(byteArray)}");
                                }
                            }
                            break;
                        case IppAttributeValueKind.Integer:
                            var intValues = attribute.Value.GetIntegerArray();
                            foreach (var intValue in intValues)
                            {
                                Console.WriteLine($"Value: {intValue}");
                            }

                            break;
                        case IppAttributeValueKind.TextWithoutLanguage:
                            var textWithoutLanguageValues = attribute.Value.GetTextWithoutLanguageArray();
                            foreach (var textValue in textWithoutLanguageValues)
                            {
                                Console.WriteLine($"Value: {textValue}");
                            }
                            break;
                        case IppAttributeValueKind.TextWithLanguage:
                            var textLanguageValues = attribute.Value.GetTextWithLanguageArray();
                            foreach (var textValue in textLanguageValues)
                            {
                                Console.WriteLine($"Value: {textValue}");
                            }
                            break;
                        case IppAttributeValueKind.Boolean:
                            var boolValues = attribute.Value.GetBooleanArray();
                            foreach (var boolValue in boolValues)
                            {
                                Console.WriteLine($"Boolean Value: {boolValue}");
                            }
                            break;
                        case IppAttributeValueKind.Charset:
                            var charsetValues = attribute.Value.GetCharsetArray();
                            foreach (var charsetValue in charsetValues)
                            {
                                Console.WriteLine($"Charset Value: {charsetValue}");
                            }
                            break;
                        case IppAttributeValueKind.NaturalLanguage:
                            var naturalLanguageValues = attribute.Value.GetNaturalLanguageArray();
                            foreach (var naturalLanguageValue in naturalLanguageValues)
                            {
                                Console.WriteLine($"NaturalLanguage Value: {naturalLanguageValue}");
                            }
                            break;
                        case IppAttributeValueKind.DateTime:
                            var dateTimeValues = attribute.Value.GetDateTimeArray();
                            foreach (var dateTimeValue in dateTimeValues)
                            {
                                Console.WriteLine($"DateTime Value: {dateTimeValue}");
                            }
                            break;
                        case IppAttributeValueKind.Resolution:
                            var resolutionValues = attribute.Value.GetResolutionArray();
                            foreach (var resolutionValue in resolutionValues)
                            {
                                string unit = resolutionValue.Unit == Windows.Devices.Printers.IppResolutionUnit.DotsPerInch ? "dpi" : "dpcm";
                                Console.WriteLine($"Resolution Value: {resolutionValue.Width}x{resolutionValue.Height} {unit}");
                            }
                            break;
                        case IppAttributeValueKind.Enum:
                            var enumValues = attribute.Value.GetEnumArray();

                            if (Mappings.enumMappings.TryGetValue(attribute.Key, out var attributeMappings))
                            {
                                foreach (var enumValue in enumValues)
                                {
                                    if (attributeMappings.TryGetValue(enumValue, out var enumString))
                                    {
                                        Console.WriteLine($"Enum Value: {enumString}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Enum Value: {enumValue}");
                                    }
                                }
                            }
                            else
                            {
                                foreach (var enumValue in enumValues)
                                {
                                    Console.WriteLine($"Enum Value: {enumValue}");
                                }
                            }
                            break;
                        case IppAttributeValueKind.Keyword:
                            var keywordValues = attribute.Value.GetKeywordArray();
                            foreach (var keywordValue in keywordValues)
                            {
                                Console.WriteLine($"Keyword Value: {keywordValue}");
                            }
                            break;
                        case IppAttributeValueKind.MimeMediaType:
                            var mimeMediaTypeValues = attribute.Value.GetMimeMediaTypeArray();
                            foreach (var mimeMediaTypeValue in mimeMediaTypeValues)
                            {
                                Console.WriteLine($"MimeMediaType Value: {mimeMediaTypeValue}");
                            }
                            break;
                        case IppAttributeValueKind.Uri:
                            var uriValues = attribute.Value.GetUriArray();
                            foreach (var uriValue in uriValues)
                            {
                                Console.WriteLine($"Uri Value: {uriValue}");
                            }
                            break;
                        case IppAttributeValueKind.UriSchema:
                            var uriSchemaValues = attribute.Value.GetUriSchemaArray();
                            foreach (var uriSchemaValue in uriSchemaValues)
                            {
                                Console.WriteLine($"UriSchema Value: {uriSchemaValue}");
                            }
                            break;
                        case IppAttributeValueKind.Collection:
                            ProcessCollection(attribute);
                            break;
                        default:
                            Console.WriteLine("Unknown");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Collection view exception: " + ex.Message);
            }
        }


        private static void ProcessCollection(KeyValuePair<string, IppAttributeValue> attribute)
        {
            Console.WriteLine($"Collection Name: {attribute.Key.ToString()}:");
            var IppValueType = attribute.Value.Kind;
            var collectionValues = attribute.Value.GetCollectionArray();

            try
            {
                foreach (var collectionValue in collectionValues)
                {
                    // Get the collection
                    var collection = collectionValue.Values;
                    Console.WriteLine($"There are {collection.Count()} member attributes in this collection");
                    foreach (var memberAttribute in collection)
                    {
                        switch (memberAttribute.Kind)
                        {
                            case IppAttributeValueKind.Integer:
                                var intVals = memberAttribute.GetIntegerArray();
                                Console.WriteLine("Integer member attribute");
                                foreach (int val in intVals)
                                {
                                    Console.WriteLine($"Integer value: {val}");
                                }
                                break;
                            case IppAttributeValueKind.Enum:
                                var enumVals = memberAttribute.GetEnumArray();
                                Console.WriteLine("Enum member attribute");
                                foreach (int val in enumVals)
                                {
                                    Console.WriteLine($"Enum value: {val}");
                                }
                                break;
                            case IppAttributeValueKind.Keyword:
                                var kwVals = memberAttribute.GetKeywordArray();
                                Console.WriteLine("Keyword member attribute");
                                foreach (string val in kwVals)
                                {
                                    Console.WriteLine($"Keyword value: {val}");
                                }
                                break;
                            case IppAttributeValueKind.Boolean:
                                Console.WriteLine($"Member Attribute Value: {memberAttribute.GetBooleanArray()}");
                                var boolVals = memberAttribute.GetBooleanArray();
                                Console.WriteLine("Boolean member attribute");
                                foreach (bool val in boolVals)
                                {
                                    Console.WriteLine(val.ToString());
                                }
                                break;
                            case IppAttributeValueKind.Charset:
                                var charVals = memberAttribute.GetCharsetArray();
                                Console.WriteLine($" Charset member attribute value");
                                foreach (string val in charVals)
                                {
                                    Console.WriteLine($"Char value: {val}");
                                }
                                break;
                            case IppAttributeValueKind.Resolution:
                                var resVals = memberAttribute.GetResolutionArray();
                                Console.WriteLine($"Resolution member attribute value");
                                foreach (IppResolution res in resVals)
                                {
                                    Console.WriteLine($"Width: {res.Width.ToString()}");
                                    Console.WriteLine($"Height: {res.Height.ToString()}");
                                }
                                break;
                            default:
                                Console.WriteLine($"Member Attribute Value: {memberAttribute.Kind.ToString()}");
                                break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Collection view exception: " + ex.Message);
            }
       }
    }
}
