using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WSDPrinterProbe
{
    /// <summary>
    /// WsdPrinterListner
    /// 
    /// Notice:
    /// Increasing the TTL (Time-To-Live) value allows the multicast packets to travel further, reaching more printers on different networks. 
    /// However, this can also introduce variability in network conditions, such as congestion or packet loss, which might cause some previously 
    /// responding printers to not answer. Since the multicast is UDP, dropped packets for congestion or packet loss are expected and produce the 
    /// relatively unpredictable results you may see.
    /// </summary>
    public class WsdPrinterListner
    {
        public static IPEndPoint remoteEP = null;
        public const int UDP_PORT = 3702;
        public const int RETURN_PORT = 55123;
        private int m_iHops = 2;
        public static IPEndPoint e = null;
        private const string sProbe_Message = "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsa=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:wsd=\"http://schemas.xmlsoap.org/ws/2005/04/discovery\" xmlns:wsdp=\"http://schemas.xmlsoap.org/ws/2006/02/devprof\" xmlns:user=\"http://schemas.microsoft.com/windows/2006/08/wdp/print\"><soap:Header><wsa:To>urn:schemas-xmlsoap-org:ws:2005:04:discovery</wsa:To><wsa:Action>http://schemas.xmlsoap.org/ws/2005/04/discovery/Probe</wsa:Action><wsa:MessageID>urn:uuid:453c01dc-dffb-4cf6-84a9-de6274211482</wsa:MessageID></soap:Header><soap:Body><wsd:Probe><wsd:Types>wsdp:Device user:PrintDeviceType</wsd:Types></wsd:Probe></soap:Body></soap:Envelope>";
        public static IPHostEntry ipHostInfo;
        private List<string> lstProbeReturns = new List<string>();
        public int Router_Hops { get => m_iHops; set => m_iHops = value; }
        public List<string> WSDProbeReturns { get => lstProbeReturns; set => lstProbeReturns = value; }

        public WsdPrinterListner()
        {
            IPAddress ipLocal =  GetLocalIPAddress();
            e = new IPEndPoint(ipLocal, RETURN_PORT);
            IPAddress address = IPAddress.Parse("239.255.255.250");
            remoteEP = new IPEndPoint(address, UDP_PORT);
        }

        private void sendWsdQuery(UdpClient client)
        {
            Console.WriteLine("Sending WSD Probe...");
            byte[] bytes = Encoding.UTF8.GetBytes(sProbe_Message);
            client.Send(bytes, bytes.Length, remoteEP);
        }

        private IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return ipAddress;
        }

        public async Task StartListeningWithTimeout(int timeout)
        {
            using (UdpClient client = new UdpClient())
            {
                client.Client.ReceiveBufferSize = 250000;
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                client.ExclusiveAddressUse = false;
                client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, Router_Hops);
                client.Client.IOControl(-1744830452, new byte[4], null);
                client.Client.Bind(e);

                //Send the MC WSD probe
                sendWsdQuery(client);

                try
                {
                    while (true)
                    {
                        var receiveTask = client.ReceiveAsync();
                        if (await Task.WhenAny(receiveTask, Task.Delay(timeout)) == receiveTask)
                        {
                            //data received
                            UdpReceiveResult result = receiveTask.Result;
                            string receiveString = Encoding.ASCII.GetString(result.Buffer);
                            ProcessXmlMessage(ref receiveString);
                        }
                        else
                        {
                            Console.WriteLine("UDP listening function timed out");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }

        /// <summary>
        /// ProcessXmlMessage
        /// 
        /// 
        /// Called by WSD_Probe_Response to deceipher returned xml string
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessXmlMessage(ref string msg)
        {
            string s = string.Empty;
            int index = msg.IndexOf("<wsd:XAddrs>");
            if (index != -1)
            {
                int num2 = msg.IndexOf("/</wsd:XAddrs>");
                if (num2 != -1)
                {
                    index += 0x13;
                    s = msg.Substring(index, num2 - index);
                }
                else
                {
                    Console.WriteLine("Could not retrieve address from probe match");
                }
            }
            else
            {
                Console.WriteLine("Could not retrieve wsd:XAddrs from probe match");
            }

            index = msg.IndexOf("<wsa:Address>");
            if (index != -1)
            {
                int num2 = msg.IndexOf("</wsa:Address>");
                if (num2 != -1)
                {
                    index += 0x11;
                    string u = msg.Substring(index, num2 - index);
                    WSDProbeReturns.Add(s + "," + u);
                }
                else
                {
                    Console.WriteLine("Could not retrieve device uuid from probe match");
                }
            }
            else
            {
                Console.WriteLine("Could not retrieve wsd:XAddress from probe match");
            }
        }
    }
}
