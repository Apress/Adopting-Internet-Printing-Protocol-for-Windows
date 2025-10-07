using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net.Http;
using System.Xml.Linq;


namespace WSDPrinterProbe
{
    public class WsdPrinterDirected
    {

        private string sMetaData_Message = $@"<?xml version=""1.0"" encoding=""utf-8""?>
        <soap:Envelope 
        xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" 
        xmlns:wsa=""http://schemas.xmlsoap.org/ws/2004/08/addressing"">
        <soap:Header>
        <wsa:To>urn:TARGET_UUID</wsa:To>
        <wsa:Action>http://schemas.xmlsoap.org/ws/2004/09/transfer/Get</wsa:Action>
        <wsa:MessageID>urn:uuid:94ebd208-4d43-461c-b4a4-aa61ea4e77ff</wsa:MessageID>
        <wsa:ReplyTo><wsa:Address>http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</wsa:Address></wsa:ReplyTo>
        <wsa:From><wsa:Address>DE122CA2-BED7-401E-8F90-BEAFDF444D3B</wsa:Address></wsa:From>
        </soap:Header><soap:Body/></soap:Envelope>";

        private string sGetActionRequest;


        public WsdPrinterDirected()
        {
            sGetActionRequest = string.Empty;
        }


        public async Task GetPrinterInfo(WsdPrinter printer)
        {
            sGetActionRequest = sMetaData_Message.Replace("TARGET_UUID", printer.Guid);
         
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("WSDAPI");
                client.BaseAddress = new Uri("http://" + printer.Uri);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://" + printer.Uri))
                {
                    Content = new StringContent(sGetActionRequest, Encoding.UTF8, "application/soap+xml")
                };

                try
                {
                    HttpResponseMessage response = await client.SendAsync(request);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    XDocument doc = XDocument.Parse(responseContent);
                    ParseWsdGetAction(doc, printer);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error::GetPrinterInfo, directed Get request to {printer.Uri} failed.");
                }
            }
        }

        private void ParseWsdGetAction(XDocument xdoc, WsdPrinter p)
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (var xmlReader = xdoc.CreateReader())
            {
                xmlDoc.Load(xmlReader);
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("soap", "http://www.w3.org/2003/05/soap-envelope");
            nsmgr.AddNamespace("wsdp", "http://schemas.xmlsoap.org/ws/2006/02/devprof");
            nsmgr.AddNamespace("PNPX", "http://schemas.microsoft.com/windows/pnpx/2005/10");

            string[] elementNames = {
            "wsdp:FriendlyName",
            "wsdp:FirmwareVersion",
            "wsdp:SerialNumber",
            "wsdp:ModelName",
            "wsdp:ModelNumber",
            "wsdp:ModelUrl",
            "wsdp:PresentationUrl",
            "wsdp:ManufacturerUrl",
            "wsdp:ServiceId",
            "PNPX:HardwareId",
            "PNPX:CompatibleId"
            };

            foreach (string elementName in elementNames)
            {
                XmlNode node = xmlDoc.SelectSingleNode("//" + elementName, nsmgr);
                if (node != null)
                {
                    p.AddWsdXmlProperty(elementName, node.InnerText);
                }
            }
        }
    }
}
