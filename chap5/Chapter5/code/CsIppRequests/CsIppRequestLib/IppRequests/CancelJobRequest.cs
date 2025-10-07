using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CsIppRequestLib.IppRequest;

namespace CsIppRequestLib
{
    public class CancelJobRequest : IppRequest
    {
        private readonly byte[] m_bIppVersionAndOperationId = new byte[] { 0x01, 0x01, 0x00, (byte)RequestType.CANCEL_JOB }; // IPP version and operation ID
        private readonly int m_iJobId;
        private readonly object _lock = new object();

        public CancelJobRequest(string ipp_version, string printerUri, bool encrypted, int requestId, int jobId) : base(printerUri, encrypted, requestId)
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
            MakeRequestByteBuffer();
        }

        public override async Task<CompletionStruct> SendRequestAsync()
        {
            try
            {
                return await SendIppRequestAsync(RequestType.CANCEL_JOB);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void MakeRequestByteBuffer()
        {
            lock (_lock)
            {
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
                   //Add the job-attributes
                   .Concat(m_bEndOfAttributesTag)
                   .ToArray();
            }
        }
    }
}
