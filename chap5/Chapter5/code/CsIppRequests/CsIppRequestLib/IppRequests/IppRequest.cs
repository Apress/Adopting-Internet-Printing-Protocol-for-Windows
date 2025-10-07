using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Runtime.Remoting.Messaging;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public abstract class IppRequest
    {
        private string m_sPrinterUri;

        public static readonly byte[] m_bEndOfAttributesTag = new byte[]{ 0x03 }; // End of attributes tag
        public static readonly byte[] m_bJobAttributesStart = new byte[] { 0x02 };
        public static readonly byte[] m_bOperationAttributesTag = new byte[] { 0x01 }; // Operation attributes tag
        protected readonly byte[] m_bRequestId= null; // Request ID
        public const int INVALID_JOB_NUMBER = -1;
        protected byte[] m_bRequestPayload = null;
        protected byte[] m_bPrinterUri = null;
        //--Op attributes--//
        protected byte[] m_bAttributesCharset = null;
        protected byte[] m_bAttributesNaturalLanguage = null;
        protected byte[] m_bPrinterUriAttribute = null;
        protected byte[] m_bRequestingUserName = null;
        private bool m_bEncrypted = false;


        public enum RequestType : byte
        {
            PRINT_JOB = 0x02,
            VALIDATE_JOB = 0x04,
            CREATE_JOB = 0x05,
            SEND_DOCUMENT = 0x06,
            CANCEL_JOB = 0x08
        }

        /// <summary>
        /// IppRequest
        /// </summary>
        /// <param name="printer"></param>
        /// <param name="encrypted"></param>
        /// <param name="request_id"></param>
        public IppRequest(string printer, bool encrypted, int request_id)
        {
            m_bEncrypted = encrypted;
            //Port 631 per RFC 7472
            m_sPrinterUri = encrypted ? $"https://{printer}:631/ipp/print" : $"http://{printer}:631/ipp/print";
            string m_sIppUri = encrypted ? $"ipps://{printer}:631/ipp/print" : $"ipp://{printer}:631/ipp/print";
            m_bRequestId = ToBigEndianBytes(request_id);
            MakeOperationAttributes(m_sIppUri);
        }

        public abstract Task<CompletionStruct> SendRequestAsync();


        /// <summary>
        /// MakeOperationAttributes
        /// </summary>
        /// <param name="printerUri"></param> 
        private void MakeOperationAttributes(string printerUri)
        {
            // Create attributes with dynamic length calculation

            m_bAttributesCharset = RequestHelpers.CreatePrinterAttribute(0x47, "attributes-charset", "utf-8");
            m_bAttributesNaturalLanguage = RequestHelpers.CreatePrinterAttribute(0x48, "attributes-natural-language", "en");
            m_bPrinterUriAttribute = RequestHelpers.CreatePrinterAttribute(0x45, "printer-uri", printerUri);
            m_bRequestingUserName = RequestHelpers.CreatePrinterAttribute(0x42, "requesting-user_name", Environment.UserName);
        }


        private byte[] ToBigEndianBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        protected Dictionary<string, byte[]> IppVersions = new Dictionary<string, byte[]>()
        {
            { "1.0", new byte[] { 0x01, 0x00 } },
            { "1.1", new byte[] { 0x01, 0x01 } },
            { "2.0", new byte[] { 0x02, 0x00 } },
            { "2.2", new byte[] { 0x02, 0x02 } }
        };

        /// <summary>
        /// SendIppRequest
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected async Task<CompletionStruct> SendIppRequestAsync(RequestType type)
        {
            // Set timeout default at 30 seconds. For printers sleeping, you might have to adjust this...
            var handler = new HttpClientHandler();
            if (m_bEncrypted)
            {
                //Note: the following trusts all certs from all printers - you might want to change this for production.
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }

            using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) })
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    var content = new ByteArrayContent(m_bRequestPayload);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/ipp");

                    try
                    {
                        var response = await client.PostAsync(m_sPrinterUri, content, cts.Token);
                        return await GetIppResponseAsync(type, response);
                    }
                    catch (TaskCanceledException)
                    {
                        throw new Exception("Request timed out.");
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            throw new Exception($"An error occurred: {ex.InnerException.Message}");
                        }
                        else
                        {
                            throw new Exception($"An error occurred: {ex.Message}");
                        }
                    }
                }
            }
        }

        protected async Task<CompletionStruct> GetIppResponseAsync(RequestType type, HttpResponseMessage response)
        {
            CompletionStruct cs = new CompletionStruct();
            cs.jobId = -1;
            try
            {
                // Read the response stream asynchronously
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    // Create a buffer to hold the data
                    byte[] buffer = new byte[2];

                    // Read the first 2 bytes (Version)
                    await responseStream.ReadAsync(buffer, 0, 2).ConfigureAwait(false);
                    short Version = ByteOrder.Flip(BitConverter.ToInt16(buffer, 0));
                    var MajorByte = (byte)(Version >> 8 & 0xFF);
                    var MinorByte = (byte)(Version & 0xFF);
                    string sVersion = string.Format("{0}.{1}", MajorByte.ToString(), MinorByte.ToString());

                    // Read the next 2 bytes (StatusCode)
                    await responseStream.ReadAsync(buffer, 0, 2).ConfigureAwait(false);
                    short StatusCode = ByteOrder.Flip(BitConverter.ToInt16(buffer, 0));

                    if (StatusCode > 0xff)
                    {
                        throw new Exception(string.Format("Ipp response error, status code was: {0}", StatusCode));
                    }
                    else
                    {
                        Console.WriteLine($"Status: {StatusCode}");
                    }

                    if ((type == RequestType.CREATE_JOB) || (type == RequestType.PRINT_JOB))
                    {
                        try
                        {
                            cs.jobId = await ResponseHelpers.GetIppAttributesAsync(responseStream);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error retrieving the job id: {ex.Message}");
                        }
                    }
                    else
                    {
                        cs.status = StatusCode;
                    }
                }
                return cs;
            }
            catch (Exception ex)
            {
                throw new Exception($"HttpResponseMessage exception thrown - error reading response, reason: {ex.Message}");
            }
        }



        /// <summary>
        /// GetMimeType
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        protected string GetMimeType(string filename)
        {
            string sExtension = Path.GetExtension(filename);

            if (sExtension != string.Empty)
            {
                // Guess the MIME media type based on the Extensionension...
                if (string.Compare(sExtension, ".gif", true) == 0)
                    return "image/gif";
                else if ((string.Compare(sExtension, ".htm", true) == 0) || (string.Compare(sExtension, ".htm.gz", true) == 0) || (string.Compare(sExtension, ".html", true) == 0) || (string.Compare(sExtension, ".html.gz", true) == 0))
                    return "text/html";
                else if ((string.Compare(sExtension, ".jpg", true) == 0) || string.Compare(sExtension, ".jpeg", true) == 0)
                    return "image/jpeg";
                else if ((string.Compare(sExtension, ".pcl", true) == 0) || string.Compare(sExtension, ".pcl.gz", true) == 0)
                    return "application/vnd.hp-PCL";
                else if ((string.Compare(sExtension, ".pdf", true) == 0))
                    return "application/pdf";
                else if (string.Compare(sExtension, ".png", true) == 0)
                    return "image/png";
                else if ((string.Compare(sExtension, ".ps", true) == 0) || string.Compare(sExtension, ".ps.gz", true) == 0)
                    return "application/postscript";
                else if ((string.Compare(sExtension, ".pwg", true) == 0) || (string.Compare(sExtension, ".pwg.gz", true) == 0) || (string.Compare(sExtension, ".ras", true) == 0) || (string.Compare(sExtension, ".ras.gz", true) == 0))
                    return "image/pwg-raster";
                else if ((string.Compare(sExtension, ".pxl", true) == 0) || string.Compare(sExtension, ".pxl.gz", true) == 0)
                    return "application/vnd.hp-PCLXL";
                else if ((string.Compare(sExtension, ".tif", true) == 0) || string.Compare(sExtension, ".tiff", true) == 0)
                    return "image / tiff";
                else if ((string.Compare(sExtension, ".txt", true) == 0) || string.Compare(sExtension, ".txt.gz", true) == 0 || string.Compare(sExtension, ".csv", true) == 0)
                    return "text/plain";
                else if ((string.Compare(sExtension, ".urf", true) == 0) || string.Compare(sExtension, ".urf.gz", true) == 0)
                    return "image/urf";
                else if ((string.Compare(sExtension, ".xps", true) == 0))
                    return "application/openxps";
                else
                    return "application/octet-stream";
            }
            else
            {
                // Use the "auto-type" MIME media type...
                return "application/octet-stream";
            }
        }
    }
}
