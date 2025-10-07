using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CsIppRequestLib
{
    public class GetJobsRequest : IppRequest
    {
        private readonly byte[] m_bIppVersionAndOperationId = new byte[] { 0x02, 0x00, 0x00, (byte)RequestType.GET_JOBS}; // IPP version and operation ID
        private readonly object _lock = new object();

        public string[] requestedAttributeNames =
        {
             "job-id", "job-uri", "job-printer-uri", "job-more-info",  "job-originating-user-name",  "job-state", "job-state-reasons", "job-state-message", "time-at-completed"
        };
        

        public GetJobsRequest(string ipp_version, string printerUri, bool encrypted, int requestId, bool bCompleted, bool myjobsonly) : base(printerUri, encrypted, requestId)
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
            MakeRequestByteBuffer(bCompleted, myjobsonly);
        }

        public override async Task<CompletionStruct> SendRequestAsync()
        {
            try
            {
                var provider = new ConsoleCredentialsProvider();
                return await SendIppRequestWithAuthAsync(RequestType.GET_JOBS, provider);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        /// <summary>
        /// Return an enumerator (read-only) to the collection of printer attributes
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public IEnumerator<IppAttribute> GetAttributeValues()
        {
            return m_pac.GetEnumerator();
        }

        private void MakeRequestByteBuffer(bool completed, bool myjobs)
        {
            lock (_lock)
            {
                byte[] bLimitAttribute = RequestHelpers.CreateIppAttribute((byte)AttributeHelper.ValueTag.Integer, "limit", 60);
                byte[] bWhichJobsAttribute = RequestHelpers.CreateIppAttribute((byte)AttributeHelper.ValueTag.Keyword, "which-jobs", completed == false ? "not-completed" : "completed");
                byte[] bMyJobsAttribute = RequestHelpers.CreateIppAttribute((byte)AttributeHelper.ValueTag.Boolean, "my-jobs", myjobs);

                // Initialize a list to accumulate the requested attributes
                List<byte> requestedAttributesBuffer = new List<byte>();
                foreach (string attr in requestedAttributeNames)
                {
                    var bufbuf = RequestHelpers.CreateIppAttribute((byte)AttributeHelper.ValueTag.Keyword, "requested-attributes", attr);
                    requestedAttributesBuffer.AddRange(bufbuf);
                }

                // Build final payload
                m_bRequestPayload = m_bIppVersionAndOperationId
                    .Concat(m_bRequestId)
                    .Concat(m_bOperationAttributesTag)
                    .Concat(m_bAttributesCharset)
                    .Concat(m_bAttributesNaturalLanguage)
                    .Concat(m_bPrinterUriAttribute)
                    .Concat(m_bRequestingUserName)
                    .Concat(bLimitAttribute)
                    .Concat(bWhichJobsAttribute)
                    .Concat(bMyJobsAttribute)
                    .Concat(requestedAttributesBuffer.ToArray())
                    .Concat(m_bEndOfAttributesTag)
                    .ToArray();
            }
        }
    }
}
