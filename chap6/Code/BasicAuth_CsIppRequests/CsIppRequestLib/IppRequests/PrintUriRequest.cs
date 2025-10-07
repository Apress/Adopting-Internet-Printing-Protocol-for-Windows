using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public class PrintUriRequest : IppRequest
    {
        private readonly byte[] m_bIppVersionAndOperationId = new byte[] { 0x01, 0x01, 0x00, (byte)RequestType.PRINT_URI }; // IPP version and operation ID
        private readonly object _lock = new object();

        public PrintUriRequest(string ipp_version, string printerUri, bool encrypted, int requestId, string file_uri, Dictionary<string, List<object>> jobAttributes) : base(printerUri, encrypted, requestId)
        {
            if (IppVersions.ContainsKey(ipp_version))
            {
                m_bIppVersionAndOperationId[0] = IppVersions[ipp_version][0];
                m_bIppVersionAndOperationId[1] = IppVersions[ipp_version][1];
            }
            else
            {
                throw new Exception($"Ipp verision: {ipp_version} is not supported");
            }
            m_pac = new IppAttributes();
            MakeRequestByteBuffer(file_uri, jobAttributes);
        }

        /// <summary>
        /// SendPrintJobRequest
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override async Task<CompletionStruct> SendRequestAsync()
        {
            try
            {
                var provider = new ConsoleCredentialsProvider();
                return await SendIppRequestWithAuthAsync(RequestType.PRINT_URI, provider);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// MakeRequestByteBuffer
        /// </summary>
        private void MakeRequestByteBuffer(string file_uri, Dictionary<string, List<object>> _jobAttributes)
        {
            lock (_lock)
            {
                string ext = GetMimeType(file_uri);
                var docFormatAttribute = RequestHelpers.CreateIppAttribute((byte)AttributeHelper.ValueTag.MimeMediaType, "document-format", ext);
                var fileUriAttribute = RequestHelpers.CreateIppAttribute((byte)AttributeHelper.ValueTag.Uri, "document-uri", file_uri);
                //Create job attributes byte buffer
                byte[] concatenatedArray = RequestHelpers.CreateJobAttributesByteArray(_jobAttributes);

                m_bRequestPayload = m_bIppVersionAndOperationId
                   .Concat(m_bRequestId)
                   .Concat(m_bOperationAttributesTag)
                   // Add the attributes
                   .Concat(m_bAttributesCharset)
                   .Concat(m_bAttributesNaturalLanguage)
                   .Concat(m_bPrinterUriAttribute)
                   .Concat(m_bRequestingUserName)
                   .Concat(docFormatAttribute)
                   .Concat(fileUriAttribute)
                   //Add the job-attributes
                   .Concat(m_bJobAttributesStart)
                   .Concat(concatenatedArray)
                   .Concat(m_bEndOfAttributesTag)
                   .ToArray();
            }
        }
    }
}
