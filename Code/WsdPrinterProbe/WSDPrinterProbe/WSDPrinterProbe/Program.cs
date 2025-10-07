
namespace WSDPrinterProbe
{
    using System;
    using System.Threading.Tasks;

    internal class Program
    {
        /// Notice:
        /// Increasing the TTL (Time-To-Live) value allows the multicast packets to travel further, reaching more printers on different networks. 
        /// However, this can also introduce variability in network conditions, such as congestion or packet loss, which might cause some previously 
        /// responding printers to not answer. Since the multicast is UDP, dropped packets for congestion or packet loss are expected and produce the 
        /// relatively unpredictable results you may see.

        private static int iHops = 1;
        private const int UDP_CANCEL_TIMEOUT = 5000;
        private static WsdPrinterListner listner = null;
        private static WsdPrinters _wsdPrinters = new WsdPrinters();
        private static async Task Main(string[] args)
        {
            try
            {
                iHops = Int32.Parse(args[0]);
                if(iHops > 3)
                { 
                    iHops = 3; 
                }
            }
            catch (Exception)
            {
                iHops = 1;
            }

            try
            {
                listner = new WsdPrinterListner();
            }
            catch(Exception ex)
            {
                Console.WriteLine("WsdPrinterListner failed, reason: " + ex.Message);
                return;
            }

            //Do the WSD Multicast Probe 
            listner.Router_Hops = iHops;
            try
            {
                await listner.StartListeningWithTimeout(UDP_CANCEL_TIMEOUT);
            }
            catch(Exception ex) 
            {
                Console.WriteLine("Could not set up WSD listening. reason: " + ex.Message);
                return;
            }

            Console.WriteLine("Found {0} WSD printers", listner.WSDProbeReturns.Count);
            if (listner.WSDProbeReturns.Count > 0)
            { 
                foreach (string p in listner.WSDProbeReturns)
                {
                    string[] items = p.Split(',');
                    _wsdPrinters.AddWsdPrinter(items[0], items[1]);
                }
            }
            else 
            {
                Console.WriteLine("No UDP printers found...");
            }

            //Send directed queries
            Console.WriteLine("Sending directed requests to each probe match for printer properties..");
            foreach (WsdPrinter printer in _wsdPrinters)
            {
                try
                {
                    WsdPrinterDirected wsdpd = new WsdPrinterDirected();
                    await wsdpd.GetPrinterInfo(printer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }

            //Now print out all printers and their properties found in the WSD probe...
            Console.WriteLine("WSD printers found by multicast probe..");
            Console.WriteLine("--------------------------------------------");
            foreach (WsdPrinter printer in _wsdPrinters)
            {
                Console.WriteLine($"IP Address: {printer.IpAddress}");
                Console.WriteLine($"Uri: {printer.Uri}");
                Console.WriteLine($"Guid:{printer.Guid}");
                foreach(WsdProperty prop in printer)
                {
                    Console.WriteLine($"{prop.Name}: {prop.Value}");
                }
                Console.WriteLine("--------------------------------------------");
            }


            Console.WriteLine("fini");
        }
    }
}
