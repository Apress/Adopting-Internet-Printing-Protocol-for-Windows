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

namespace GetPrinterAttributesRequest
{
    public static class Program
    {
        private static string printer_name;
        private static string requested_string;
        private static string[] requested_attributes;
        private static List<byte> byteList = new List<byte>();
        static async Task Main(string[] args)
        {
            try
            {
                GetCommandLineArgs(args);
            }
            catch(Exception)
            {
                ShowUsage();
                return;
            }

            try 
            {
                requested_attributes = requested_string.Trim().Split(',');
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid requested attributes provided.");
                return;
            }

            var printerUri = @"http://" + printer_name + "/ipp/print";
            var printerUriBytes = Encoding.UTF8.GetBytes(printerUri);

            // Define the IPP request components
            var ippVersionAndOperationId = new byte[] { 0x01, 0x01, 0x00, 0x0B }; // IPP version and operation ID
            var requestId = new byte[] { 0x00, 0x00, 0x00, 0x01 }; // Request ID
            var operationAttributesTag = new byte[] { 0x01 }; // Operation attributes tag

            // Create attributes with dynamic length calculation
            var attributesCharset = RequestHelpers.CreateAttribute(0x47, "attributes-charset", "utf-8", true);
            var attributesNaturalLanguage = RequestHelpers.CreateAttribute(0x48, "attributes-natural-language", "en", true);
            var printerUriAttribute = RequestHelpers.CreateAttribute(0x45, "printer-uri", printerUri, true);

            
            for(int i = 0; i< requested_attributes.Length; i++)
            {
                if (i == 0)
                    byteList.AddRange(RequestHelpers.CreateAttribute(0x44, "requested-attributes", requested_attributes[i], true));
                else
                    byteList.AddRange(RequestHelpers.CreateAttribute(0x44, "requested-attributes", requested_attributes[i], false));
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
                //Add the requested-attributes
                .Concat (concatenatedArray)
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
                    Console.WriteLine("Get-Printer-Attributes Response:");
                    await GetIppResponse(response);
                    Console.WriteLine("fini");
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Request timed out.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
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
        static async Task GetIppResponse(HttpResponseMessage response)
        {
            try
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
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
                        Console.WriteLine("The version returned was {0}.{1}", MajorByte, MinorByte);
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
                        Console.WriteLine("The status code was {0}", StatusCode);
                        //Look at the next 4 bytes
                        int RequestId = ByteOrder.Flip(reader.ReadInt32());
                        Console.WriteLine("The request Id was {0}", RequestId);
                        //Get the rest of the IPP response
                        ResponseHelpers.GetIppAttributes(reader);
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("HttpResponseMessage exception thrown - error reading response, reason: {0}", ex.Message));
            }

        }

        /// <summary>
        /// 
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

        static void ShowUsage()
        {
            Console.WriteLine("Usage: GetPrinterAttributesRequest /p=<printer_name> /r=<requested_attributes");
            Console.WriteLine("Where <printer_name> is the name of the target printer");
            Console.WriteLine("Where <requested_attributes> is a comma-delimited string of requested attributes");
            Console.WriteLine("The requested attributes should be comma-delimited with no spaces between attributes");
            Console.WriteLine("Example: GetPrinterAttributesRequest /p=printer1 /r=printer-description,job-template,media-col-database");
            return;
        }

    }
}



