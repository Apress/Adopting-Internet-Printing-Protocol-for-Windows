using AddSharedIppPrinters.Helpers;
using AddSharedIppPrinters.Print;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AddSharedIppPrinters
{
    public class PrinterConnections: IEnumerable<PrinterConnection> 
    {

        List<PrinterConnection> m_connections = new List<PrinterConnection> ();

        public PrinterConnections()
        {
        }     

        public List<PrinterConnection> Printers => m_connections;
        
        public void AddIppPrinterConnection(PrinterConnection pc)
        {
            if (Find(pc.PRINTERNAME) != null)
            {
                return;
            } 
            else
            {
                Printers.Add(pc);
            }
        }

        public void Clear()
        {  
            Printers.Clear(); 
        }

        public IEnumerator<PrinterConnection> GetEnumerator()
        {
            return m_connections.GetEnumerator();   
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(); 
        }

        private PrinterConnection Find(string name)
        {
            return m_connections.FirstOrDefault(x => String.Equals(x.PRINTERNAME, name, StringComparison.CurrentCultureIgnoreCase)); 
        }
    }
}
