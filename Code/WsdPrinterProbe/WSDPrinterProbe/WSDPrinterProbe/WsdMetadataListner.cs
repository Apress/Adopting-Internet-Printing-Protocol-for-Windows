using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace WSDPrinterProbe
{
    public class WsdMetadataListner
    {
        private string m_sPrinterIP = string.Empty;
        private string m_sUUID = string.Empty;
        private string m_sPrinterUrl = string.Empty;

        private string sMetaData_Message  = $@"
        <s:Envelope xmlns:s=""http://www.w3.org/2003/05/soap-envelope"">
            <s:Header>
                <a:Action s:mustUnderstand=""1"" xmlns:a=""http://www.w3.org/2005/08/addressing"">http://schemas.xmlsoap.org/ws/2004/09/mex/GetMetadata/Request</a:Action>
                <a:MessageID xmlns:a=""http://www.w3.org/2005/08/addressing"">urn:uuid:{Guid.NewGuid()}</a:MessageID>
                <a:To s:mustUnderstand=""1"" xmlns:a=""http://www.w3.org/2005/08/addressing"">http://PRINTER_IP_ADDRESS:5357/PRINTER_UUID</a:To>
            </s:Header>
            <s:Body>
                <GetMetadata xmlns=""http://schemas.xmlsoap.org/ws/2004/09/mex""/>
            </s:Body>
        </s:Envelope>";

        public WsdMetadataListner(string ip_address, string device_uuid)
        {
            PrinterIP = ip_address;
            UUID = device_uuid;
            sMetaData_Message = sMetaData_Message.Replace("PRINTER_IP_ADDRESS", PrinterIP);
            sMetaData_Message = sMetaData_Message.Replace("PRINTER_UUID", UUID);
            PrinterUrl = @"http://" + PrinterIP + ":5357/" + UUID;
        }

        public string PrinterIP { get => m_sPrinterIP; set => m_sPrinterIP = value; }
        public string UUID { get => m_sUUID; set => m_sUUID = value; }
        public string PrinterUrl { get => m_sPrinterUrl; set => m_sPrinterUrl = value; }

        public async Task StartListeningForMetaResponse()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, PrinterUrl)
                {
                    Content = new StringContent(sMetaData_Message, Encoding.UTF8, "application/soap+xml")
                };

                try
                {
                    HttpResponseMessage response = await client.SendAsync(request);
                    string responseContent = await response.Content.ReadAsStringAsync();

                    XDocument doc = XDocument.Parse(responseContent);
                    Console.WriteLine(doc);
                }
                catch(Exception ex) 
                { 
                    Console.WriteLine(ex.ToString());       
                }
            }

        }
    }
}
