using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public abstract class IppRequest
    {
        protected IppAttributes m_pac;
        private readonly string m_sPrinterUri;
        public static readonly byte[] m_bEndOfAttributesTag = new byte[] { 0x03 }; // End of attributes tag
        public static readonly byte[] m_bJobAttributesStart = new byte[] { 0x02 };
        public static readonly byte[] m_bOperationAttributesTag = new byte[] { 0x01 }; // Operation attributes tag
        protected readonly byte[] m_bRequestId = null; // Request ID
        public const int INVALID_JOB_NUMBER = -1;
        protected byte[] m_bRequestPayload = null;
        protected byte[] m_bPrinterUri = null;
        //--Op attributes--//
        protected byte[] m_bAttributesCharset = null;
        protected byte[] m_bAttributesNaturalLanguage = null;
        protected byte[] m_bPrinterUriAttribute = null;
        protected byte[] m_bRequestingUserName = null;
        protected bool m_bChunkRequest = false;


        public enum RequestType : byte
        {
            PRINT_JOB = 0x02,
            PRINT_URI = 0x03,
            VALIDATE_JOB = 0x04,
            CREATE_JOB = 0x05,
            SEND_DOCUMENT = 0x06,
            CANCEL_JOB = 0x08,
            GET_JOBS = 0x0A,
            GET_PRINTER_ATTRIBUTES = 0x0B,
            IDENTIFY_PRINTER = 0x3C,
            GET_JOB_ATTRIBUTES = 0x09,
	        PAUSE_PRINTER = 0x10,
	        RESUME_PRINTER = 0x11
        }

        /// <summary>
        /// IppRequest
        /// </summary>
        /// <param name="printer"></param>
        /// <param name="encrypted"></param>
        /// <param name="request_id"></param>
        public IppRequest(string printer, bool encrypted, int request_id)
        {
            bool m_bEncrypted = encrypted;
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

        /// <summary>
        /// IppVersions
        /// </summary>
        protected Dictionary<string, byte[]> IppVersions = new Dictionary<string, byte[]>()
        {
            { "1.0", new byte[] { 0x01, 0x00 } },
            { "1.1", new byte[] { 0x01, 0x01 } },
            { "2.0", new byte[] { 0x02, 0x00 } },
            { "2.2", new byte[] { 0x02, 0x02 } }
        };

        public IppAttributes Pac { get => m_pac; }


        /// <summary>
        /// SendIppRequest
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected async Task<CompletionStruct> SendIppRequestWithAuthAsync(RequestType type, ICredentialsProvider credentialsProvider)
        {
            bool isAuthenticated = false;
            var client = HttpClientFactory.GetHttpClient(m_sPrinterUri);
            CompletionStruct cs = new CompletionStruct(); 

            // Loop until we either authenticate successfully or the user cancels
            while (!isAuthenticated)
            {

                //Default to use 30 seconds as timeout for all requests - change this as needed..
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    HttpContent content;
                    if (m_bChunkRequest)
                    {
                        // Chunk the request
                        var stream = new MemoryStream(m_bRequestPayload);
                        content = new StreamContent(stream);
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/ipp");
                    }
                    else
                    {
                        content = new ByteArrayContent(m_bRequestPayload);
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/ipp");
                    }

                    // Create a new HttpRequestMessage and set request-specific headers on it
                    var request = new HttpRequestMessage(HttpMethod.Post, m_sPrinterUri)
                    {
                        Content = content
                    };

                    if (m_bChunkRequest)
                    {
                        // If needed, set the Transfer-Encoding header directly on the message
                        request.Headers.TransferEncodingChunked = true;
                    }

                    // Send the request
                    HttpResponseMessage response;
                    try
                    {
                        response = await client.SendAsync(request, cts.Token);
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

                    // If unauthorized, request credentials and update the auth header
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        // Request credentials from the provided provider
                        var (username, password, cancel) = await credentialsProvider.GetCredentialsAsync(isAuthenticated);

                        // If the user cancels out, then throw exception..
                        if (cancel == true)
                        {
                            throw new OperationCanceledException("User canceled credential prompt.");
                        }

                        // Set the Basic auth header and continue the loop to retry the request
                        string credentials = $"{username}:{password}";
                        string encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
                        continue;
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        // If we get a successful response, process it.
                        cs = await GetResponseAsync(type, response);
                        isAuthenticated = true;  // Exit loop
                    }
                    else
                    {
                        // Handle other unexpected status codes
                        throw new Exception($"Request failed. Status Code: {response.StatusCode}");
                    }
                }
            }
            return cs;
        }


        /// <summary>
        /// GetIppResponseAsync
        /// </summary>
        /// <param name="type"></param>
        /// <param name="response"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected async Task<CompletionStruct> GetResponseAsync(RequestType type, HttpResponseMessage response, CancellationToken ct = default)
        {
            CompletionStruct cs;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception("Unauthorized access to printer, check credentials.");

            try
            {

                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {

                    // ---- IPP response header ----
                    // (2 bytes) version
                    byte[] buffer = new byte[2];
                    await SafeStreamRead.ReadExactlyAsync(responseStream, buffer, 0, 2, ct).ConfigureAwait(false);
                    short Version = ByteOrder.Flip(BitConverter.ToInt16(buffer, 0));
                    byte major = (byte)((Version >> 8) & 0xFF);
                    byte minor = (byte)(Version & 0xFF);
                    string sVersion = $"{major}.{minor}";

                    // (2 bytes) status-code
                    await SafeStreamRead.ReadExactlyAsync(responseStream, buffer, 0, 2, ct).ConfigureAwait(false);
                    short StatusCode = ByteOrder.Flip(BitConverter.ToInt16(buffer, 0));


                    // (4 bytes) request-id  
                    byte[] b4 = new byte[4];
                    await SafeStreamRead.ReadExactlyAsync(responseStream, b4, 0, 4, ct).ConfigureAwait(false);
                    int RequestId = ByteOrder.Flip(BitConverter.ToInt32(b4, 0));

                    // Handle specific request types
                    if (type == RequestType.CREATE_JOB || type == RequestType.PRINT_JOB || type == RequestType.PRINT_URI || type == RequestType.GET_JOBS || type == RequestType.GET_JOB_ATTRIBUTES)
                    {
                        cs = await ResponseHelpers.GetIppAttributesCollectionAsync(responseStream, type, Pac, ct).ConfigureAwait(false);
                        cs.status = StatusCode;
                    }
                    else if (type == RequestType.GET_PRINTER_ATTRIBUTES)
                    {
                        cs = await ResponseHelpers.GetIppAttributesCollectionAsync(responseStream, type, m_pac, ct).ConfigureAwait(false);
                        cs.status = StatusCode;
                    }
                    else
                    {
                        cs = await ResponseHelpers.GetIppAttributesCollectionAsync(responseStream, type, m_pac, ct).ConfigureAwait(false);
                        cs.status = StatusCode;
                    }
                }
                return cs;
            }
            catch (Exception ex)
            {
                throw new Exception($"HttpResponseMessage exception thrown - error reading response, reason: {ex.Message}", ex);
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
                // Guess the MIME media type based on the Extension...
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
                    return "not_supported";
            }
            else
            {
                // Use the "auto-type" MIME media type...
                return "not_supported";
            }
        }
    }
    public class ConsoleCredentialsProvider : ICredentialsProvider
    {
        public Task<(string Username, string Password, bool bCancel)> GetCredentialsAsync(bool bIsAuthenticated)
        {
            if (bIsAuthenticated == false)
            {
                Console.WriteLine("**User is not authenticated or authentication failed**");
            }
            string sUserName = string.Empty;
            string sPassword = string.Empty;
            Console.Write("Enter Username: ");
            string username = Console.ReadLine();
            Console.Write("Enter Password: ");
            string password = Console.ReadLine();
            Console.WriteLine("Enter 'q' to cancel, or 'c' to continue:");
            if (Console.ReadLine().ToLower() == "q")
            {
                return Task.FromResult((sUserName, sPassword, true));
            }
            return Task.FromResult((username, password, false));
        }
    }
}
