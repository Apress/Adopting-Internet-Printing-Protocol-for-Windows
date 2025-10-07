using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CsIppRequestLib
{
    /// <summary>
    /// SendDocRequest
    /// 
    /// This is a recommended but not required request
    /// If the printer returns IPP_STATUS_ERROR_MULTIPLE_JOBS_NOT_SUPPORTED (1289) error, it does not support multiple docs.
    /// However, the Printer MUST accept the first Document with a ’true’ or
    /// ’false’ value for the "last-document" operation attribute(see
    /// below), so that Clients MAY always submit one Document Job with a
    /// ’false’ value for "last-document" in the first Send-Document and a
    /// ’true’ value for "last-document" in the second Send-Document(with
    /// no data).
    /// 
    /// Look for the "multiple-document-jobs-supported" attribute to see if the printer supports multiple documents in a job.
    /// The "multiple-document-handling" Job Template attribute must be used when creating a job for multiple docs in Send-Document
    /// </summary>
    public class SendDocRequest : IppRequest
    {
        private readonly byte[] m_bIppVersionAndOperationId = new byte[] { 0x01, 0x01, 0x00, (byte)RequestType.SEND_DOCUMENT }; // IPP version and operation ID
        private readonly int m_iJobId;
        private readonly string m_sFileName;
        private readonly object _lock = new object();

        public SendDocRequest(string ipp_version, string printerUri, bool encrypted, int requestId, int jobId, string fileName, bool bChunk, bool bLastDocument) : base(printerUri, encrypted, requestId)
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

            m_bChunkRequest = bChunk;
            m_iJobId = jobId;
            m_sFileName = fileName;
            if(File.Exists(fileName) == false)
            {
                throw new Exception($"Error: {fileName} could not be found!");
            }
            m_pac = new IppAttributes();
            MakeRequestByteBuffer(bLastDocument);
        }

        /// <summary>
        /// SendDocumentRequest
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override async Task<CompletionStruct> SendRequestAsync()
        {
            try
            {
                var provider = new ConsoleCredentialsProvider();
                return await SendIppRequestWithAuthAsync(RequestType.SEND_DOCUMENT, provider);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// MakeRequestByteBuffer
        /// </summary>
        private void MakeRequestByteBuffer(bool bLastJob)
        {
            lock (_lock)
            {
                byte[] fileBytes = File.ReadAllBytes(m_sFileName);
                string ext = GetMimeType(m_sFileName);
                var docFormatAttribute = RequestHelpers.CreateIppAttribute((byte)AttributeHelper.ValueTag.MimeMediaType, "document-format", ext);
                var lastDocAttribute = RequestHelpers.CreateIppAttribute((byte)AttributeHelper.ValueTag.Boolean, "last-document", bLastJob);
                byte[] m_bJobId = RequestHelpers.CreateIppAttribute((byte)AttributeHelper.ValueTag.Integer, "job-id", m_iJobId);
                //
                m_bRequestPayload = m_bIppVersionAndOperationId
                   .Concat(m_bRequestId)
                   .Concat(m_bOperationAttributesTag)
                   // Add the attributes
                   .Concat(m_bAttributesCharset)
                   .Concat(m_bAttributesNaturalLanguage)
                   .Concat(m_bPrinterUriAttribute)
                   .Concat(m_bJobId)
                   .Concat(m_bRequestingUserName)
                   .Concat(docFormatAttribute)
                   .Concat(lastDocAttribute)
                   .Concat(m_bEndOfAttributesTag)
                   .Concat(fileBytes)
                   .ToArray();
            }
        }
    }
}
