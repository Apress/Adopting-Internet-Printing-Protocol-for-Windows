using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CsIppRequestLib
{
    public class SendDocRequest : IppRequest
    {
        private readonly byte[] m_bIppVersionAndOperationId = new byte[] { 0x01, 0x01, 0x00, (byte)RequestType.SEND_DOCUMENT }; // IPP version and operation ID
        private readonly int m_iJobId;
        private readonly string m_sFileName;
        private readonly object _lock = new object();

        public SendDocRequest(string ipp_version, string printerUri, bool encrypted, int requestId, int jobId, string fileName) : base(printerUri, encrypted, requestId)
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

            m_iJobId = jobId;
            m_sFileName = fileName;
            if(File.Exists(fileName) == false)
            {
                throw new Exception($"Error: {fileName} could not be found!");
            }
            MakeRequestByteBuffer();
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
                return await SendIppRequestAsync(RequestType.SEND_DOCUMENT);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// MakeRequestByteBuffer
        /// </summary>
        private void MakeRequestByteBuffer()
        {
            lock (_lock)
            {
                byte[] fileBytes = File.ReadAllBytes(m_sFileName);
                string ext = GetMimeType(m_sFileName);
                var docFormatAttribute = RequestHelpers.CreatePrinterAttribute((byte)AttributeHelper.ValueTag.MimeMediaType, "document-format", ext);
                var lastDocAttribute = RequestHelpers.CreatePrinterAttribute((byte)AttributeHelper.ValueTag.Boolean, "last-document", true);
                byte[] m_bJobId = RequestHelpers.CreatePrinterAttribute((byte)AttributeHelper.ValueTag.Integer, "job-id", m_iJobId);
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
