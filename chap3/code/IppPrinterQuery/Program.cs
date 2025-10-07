using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Xml.Linq;
using IppPrinterQuery;
using System.Collections;
using System.Security.Cryptography;

namespace GetPrinterAttributesRequest
{
    public static class Program
    {
        private static List<string> singleAttributes = new List<string>();
        private static string attributes_file = string.Empty;
        private static string printer_list = string.Empty;
        private static List<byte> byteList = new List<byte>();
        private static IppPrinters _printers = new IppPrinters();

        static async Task Main(string[] args)
        {
;
            List<string> printers = new List<string>();
            try
            {
                GetCommandLineArgs(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                ShowUsage();
                return;
            }
            
            try 
            {
                LoadXmlFile(attributes_file);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing attributes xml file: {attributes_file}, reason: {ex.ToString()}");
                return;
            }

            try
            {
                printers = LoadPrintersList(printer_list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing printer list: {printer_list}, reason: {ex.ToString()}");
                return;
            }


            foreach(string printer in printers)
            {
                try 
                {
                    await GetIppAttributesFromPrinter(printer);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error performing Get-Printer-Attributes request on: {printer}, reason: {ex.ToString()}");
                }
            }

            Console.WriteLine($"Collected attributes from {_printers.Printers.Count} IPP printers");
            DoPrintOut();
            Console.WriteLine("----------------------------------------------------");
        }


        /// <summary>
        /// GetIppAttributesFromPrinter
        /// </summary>
        /// <param name="printer_name"></param>
        /// <returns></returns>
        static async Task GetIppAttributesFromPrinter(string printer_name)
        {
            var printerUri = @"http://" + printer_name + "/ipp/print";
            var printerUriBytes = Encoding.UTF8.GetBytes(printerUri);

            // Define the IPP request components
            //var ippVersionAndOperationId = new byte[] { 0x01, 0x01, 0x00, 0x0B }; // IPP version and operation ID
            var ippVersionAndOperationId = new byte[] { 0x02, 0x02, 0x00, 0x0B }; // IPP version and operation ID
            var requestId = new byte[] { 0x00, 0x00, 0x00, 0x01 }; // Request ID
            var operationAttributesTag = new byte[] { 0x01 }; // Operation attributes tag

            // Create attributes with dynamic length calculation
            var attributesCharset = RequestHelpers.CreatePrinterAttribute(0x47, "attributes-charset", "utf-8");
            var attributesNaturalLanguage = RequestHelpers.CreatePrinterAttribute(0x48, "attributes-natural-language", "en");
            var printerUriAttribute = RequestHelpers.CreatePrinterAttribute(0x45, "printer-uri", printerUri);

            byteList.AddRange(RequestHelpers.CreatePrinterAttribute(0x44, "requested-attributes", "printer-description"));

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
                //Add the requested-attributes
                .Concat(concatenatedArray)
                .Concat(endOfAttributesTag)
                .ToArray();

            //Set timeout default at 30 seconds. For printers sleeping, you might have to adjust this...
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                var content = new ByteArrayContent(requestPayload);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/ipp");

                try
                {
                    var response = await client.PostAsync(printerUri, content, cts.Token);
                    await GetIppResponse(response, printer_name);
                }
                catch (TaskCanceledException)
                {
                    throw new Exception("Error, the request timed out.");
                }
                catch (Exception ex)
                {
                    if(ex.InnerException != null)   
                        throw new Exception($"The error: {ex.InnerException} was throw trying to get IPP Get-Printer-Attributes response");
                    else
                        throw new Exception($"The error: {ex.Message} was throw trying to get IPP Get-Printer-Attributes response");
                }
            }
        }


        /// <summary>
        /// GetIppResponse
        /// 
        /// 
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        static async Task GetIppResponse(HttpResponseMessage response, string printerName)
        {
            try
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    using (var reader = new BinaryReader(responseStream, Encoding.ASCII, true))
                    {
                        //Look at the first 2 bytes
                        short Version = ByteOrder.Flip(reader.ReadInt16());
                        var MajorByte = (byte)(Version >> 8 & 0xFF);
                        var MinorByte = (byte)(Version & 0xFF);
                        string sVersion = string.Format("{0}.{1}", MajorByte.ToString(), MinorByte.ToString());
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
                        //Look at the next 4 bytes
                        int RequestId = ByteOrder.Flip(reader.ReadInt32());
                        //Get the attributes of this printer
                       IppPrinter ippPrinter = new IppPrinter(printerName, singleAttributes);
                        ippPrinter.StatusCode = StatusCode;
                        ippPrinter.Version = sVersion;
                        ResponseHelpers.GetIppAttributes(reader, _printers, ippPrinter);
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("HttpResponseMessage exception thrown - error reading response, reason: {0}", ex.Message));
            }

        }

        /// <summary>
        /// GetCommandLineArgs
        /// 
        ///  Ensures command line arguments are processed.
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        static void GetCommandLineArgs(string[] args)
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
                        printer_list = args[i].Substring(3);
                        count++;
                    }
                    else if (args[i].StartsWith("/f=", StringComparison.CurrentCultureIgnoreCase))
                    {
                        attributes_file = args[i].Substring(3);
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

        /// <summary>
        /// LoadXmlFile
        /// 
        /// Loads and processes the attributes you are interested in monitoring 
        /// or observing. 
        /// </summary>
        /// <param name="xml_file_path"></param>
        /// <exception cref="Exception"></exception>
        static void LoadXmlFile(string xml_file)
        {
            try
            {
                // Load the XML content
                XDocument doc = XDocument.Load(xml_file);
              
                foreach (var element in doc.Descendants("SingleAttributes").Elements())
                {
                    singleAttributes.Add(element.Name.LocalName);
                }
            }
            catch (Exception ex) 
            {
                throw new Exception($"Failed to process attributes file {xml_file}, reason: {ex.Message}");
            }   
        }

        /// <summary>
        /// LoadPrinterList()
        /// 
        /// Loads the list of printer names. The list should have one printer 
        /// per line.
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        static List<string> LoadPrintersList(string printer_list)
        {
            List<string> printers = new List<string>();

            try
            {
                string[] lines = File.ReadAllLines(printer_list);
                foreach (string line in lines)
                {
                    if (line.Trim().Length > 0)
                    {
                        printers.Add(line);
                    }
                }
                return printers;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// DoPrintOut
        /// 
        /// Example of IPP Printer collection usage. You could take the IppPrinter objects and
        /// write their contents to a db, to a WPF ObservableCollection, etc. 
        /// </summary>
        static void DoPrintOut()
        {
            foreach(IppPrinter printer in _printers)
            {
                Console.WriteLine($"------------------{printer.PrinterName}---------------------");
                Console.WriteLine($"Status Code: {printer.StatusCode}");
                Console.WriteLine($"IPP Version Used: {printer.Version}");

               foreach(IppAttributeObject ao in printer)
                {
                    Console.Write(ao.Name + ": ");
                    int numAttrs = ao.Count;
                    int num = 0;
                    foreach(string sval in ao)
                    {
                        if (sval.Trim().Length > 0)
                        {
                            num++;
                            if(num < numAttrs)
                                Console.Write(sval + ", ");
                            else
                                Console.Write(sval);
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage: IppPrinterQuery /p=<list of printers file> /f=<attributes xml file>");
            Console.WriteLine("Where <list of printers file> is the list of the target printer(s)");
            Console.WriteLine("Where <attributes xml file> is the file name and path of attributes xml file");
            Console.WriteLine("Example: IppPrinterQuery /p=C:\\Temp\\PrinterList.txt /f=C:\\Temp\\MyAttributes.xml");
            return;
        }

    }
}



