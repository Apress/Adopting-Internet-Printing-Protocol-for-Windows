using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public class ValidateJobRequest : IppRequest
    {
        private readonly byte[] m_bIppVersionAndOperationId = new byte[] { 0x01, 0x01, 0x00, (byte)RequestType.VALIDATE_JOB }; // IPP version and operation ID
        private readonly object _lock = new object();

        public ValidateJobRequest(string ipp_version, string printerUri, bool encrypted, int requestId, Dictionary<string, List<object>> jobAttributes) : base(printerUri, encrypted, requestId)
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
            MakeRequestByteBuffer(jobAttributes);
        }

        public override async Task<CompletionStruct> SendRequestAsync()
        {
            try
            {
                return await SendIppRequestAsync(RequestType.VALIDATE_JOB);
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }

        private void MakeRequestByteBuffer(Dictionary<string, List<object>> _jobAttributes)
        {
            lock (_lock)
            {
                //Create job attributes byte buffer
                byte[] concatenatedArray = RequestHelpers.CreateJobAttributesByteArray(_jobAttributes);
                //
                m_bRequestPayload = m_bIppVersionAndOperationId
                   .Concat(m_bRequestId)
                   .Concat(m_bOperationAttributesTag)
                   // Add the attributes
                   .Concat(m_bAttributesCharset)
                   .Concat(m_bAttributesNaturalLanguage)
                   .Concat(m_bPrinterUriAttribute)
                   .Concat(m_bRequestingUserName)
                   //Add the job-attributes
                   .Concat(m_bJobAttributesStart)
                   .Concat(concatenatedArray)
                   .Concat(m_bEndOfAttributesTag)
                   .ToArray();
            }
        }
    }
}
