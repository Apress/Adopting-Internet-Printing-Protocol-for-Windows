using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static CsIppRequestLib.IppRequest;

namespace CsIppRequestLib
{
    public class GetJobAttributesRequest : IppRequest
    {
        private readonly byte[] m_bIppVersionAndOperationId = new byte[] { 0x02, 0x00, 0x00, (byte)RequestType.GET_JOB_ATTRIBUTES }; // IPP version and operation ID
        private readonly int m_iJobId;
        private readonly object _lock = new object();

        string[] requestedAttributeNames =
        {
             "job-state", "job-state-reasons"
        };

        public GetJobAttributesRequest(string ipp_version, string printerUri, bool encrypted, int requestId, int jobId) : base(printerUri, encrypted, requestId)
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
            m_iJobId = jobId;   
            MakeRequestByteBuffer();
        }

        public override async Task<CompletionStruct> SendRequestAsync()
        {
            try
            {
                return await SendIppRequestAsync(RequestType.GET_JOB_ATTRIBUTES);
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

                // Initialize a list to accumulate the requested attributes
                List<byte> requestedAttributesBuffer = new List<byte>();
                foreach (string attr in requestedAttributeNames)
                {
                    var bufbuf = RequestHelpers.CreatePrinterAttribute((byte)AttributeHelper.ValueTag.Keyword, "requested-attributes", attr);
                    requestedAttributesBuffer.AddRange(bufbuf);
                }

                m_bRequestPayload = m_bIppVersionAndOperationId
                   .Concat(m_bRequestId)
                   .Concat(m_bOperationAttributesTag)
                   // Add the attributes
                   .Concat(m_bAttributesCharset)
                   .Concat(m_bAttributesNaturalLanguage)
                   .Concat(m_bPrinterUriAttribute)
                   .Concat(m_bRequestingUserName)
                   .Concat(m_bJobId)
                   .Concat(requestedAttributesBuffer.ToArray())
                   .Concat(m_bEndOfAttributesTag)
                   .ToArray();
            }
        }
    }
}
