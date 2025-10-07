using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public class GetIppAttributesRequest : IppRequest
    {
        private readonly byte[] m_bIppVersionAndOperationId = new byte[] { 0x01, 0x01, 0x00, (byte)RequestType.GET_PRINTER_ATTRIBUTES }; // IPP version and operation ID
        private readonly object _lock = new object();
        public GetIppAttributesRequest(string ipp_version, string printerUri, bool encrypted, int requestId) : base(printerUri, encrypted, requestId)
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
            MakeRequestByteBuffer();
        }

        public override async Task<CompletionStruct> SendRequestAsync()
        {
            try
            {
                var provider = new ConsoleCredentialsProvider();
                return await SendIppRequestWithAuthAsync(RequestType.GET_PRINTER_ATTRIBUTES, provider);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// IsSupported
        /// 
        /// Returns true if attribute name/value are found
        /// Returns false if attribute name/value are not found
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        public bool IsSupported(string attributeName, string attributeValue)
        {
            IppAttribute rpa = m_pac.Find(attributeName);
            if(rpa != null)
            {
                return rpa.ContainsValue(attributeValue);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// GetAttributeValues
        /// 
        /// Returns a list of attribute value strings if attribute found.
        /// Returns null if attribute is not found.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public List<string> GetAttributeValues(string attributeName)
        {
            IppAttribute rpa = m_pac.Find(attributeName);
            if (rpa != null)
            {
                return rpa.AttributeValues;
            }
            else
            {
                return null;
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

        private void MakeRequestByteBuffer()
        {
            lock (_lock)
            {
                m_bRequestPayload = m_bIppVersionAndOperationId
               .Concat(m_bRequestId)
               .Concat(m_bOperationAttributesTag)
               // Add the attributes
               .Concat(m_bAttributesCharset)
               .Concat(m_bAttributesNaturalLanguage)
               .Concat(m_bPrinterUriAttribute)
               .Concat(m_bRequestingUserName)
               .Concat(m_bEndOfAttributesTag)
               .ToArray();
            }
        }
    }
}
