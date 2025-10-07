using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CsIppRequestLib.IppRequest;

namespace CsIppRequestLib
{
    public class PrintJobRequest : IppRequest
    {
        private readonly byte[] m_bIppVersionAndOperationId = new byte[] { 0x01, 0x01, 0x00, (byte)RequestType.PRINT_JOB }; // IPP version and operation ID
        private readonly string m_sFileName;
        private readonly object _lock = new object();

        public PrintJobRequest(string ipp_version, string printerUri, bool encrypted, int requestId, string fileName, Dictionary<string, List<object>> jobAttributes) : base(printerUri, encrypted, requestId)
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

            m_sFileName = fileName;
            if (File.Exists(fileName) == false)
            {
                throw new Exception($"Error: {fileName} could not be found!");
            }
            MakeRequestByteBuffer(jobAttributes);
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
        private void MakeRequestByteBuffer(Dictionary<string, List<object>> _jobAttributes)
        {
            lock (_lock)
            {
                byte[] fileBytes = File.ReadAllBytes(m_sFileName);
                string ext = GetMimeType(m_sFileName);
                var docFormatAttribute = RequestHelpers.CreatePrinterAttribute((byte)AttributeHelper.ValueTag.MimeMediaType, "document-format", ext);
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
                   //Add the job-attributes
                   .Concat(m_bJobAttributesStart)
                   .Concat(concatenatedArray)
                   .Concat(m_bEndOfAttributesTag)
                   .Concat(fileBytes)
                   .ToArray();
            }
        }
    }
}
