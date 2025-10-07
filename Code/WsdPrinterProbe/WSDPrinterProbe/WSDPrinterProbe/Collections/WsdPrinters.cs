using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace WSDPrinterProbe
{
    public  class WsdPrinters: IEnumerable<WsdPrinter>
    {
        private List<WsdPrinter> m_lstValues = new List<WsdPrinter>();
        public WsdPrinters() 
        { 
        }

        /// <summary>
        /// AddWsdPrinter
        /// </summary>
        /// <param name="ip_address"></param>
        /// <param name="guid"></param>
        public void AddWsdPrinter(string ip_address, string guid)
        {
            if (Find(ip_address) == null)
            {
                WsdPrinter wsd_printer = new WsdPrinter(ip_address, guid);
                m_lstValues.Add(wsd_printer);
            }
        }

        public IEnumerator<WsdPrinter> GetEnumerator()
        {
            return m_lstValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Find
        /// 
        /// Get an WsdPrinter object by IP address
        /// </summary>
        /// <param name="ip_address"></param>
        /// <returns></returns>
        public WsdPrinter Find(string ip_address)
        {
            return m_lstValues.FirstOrDefault(x => String.Equals(x.IpAddress, ip_address, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
