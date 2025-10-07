using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IppPrinterQuery
{
    public  class IppPrinters :IEnumerable<IppPrinter>
    {
        private List<IppPrinter> printers = new List<IppPrinter>();
        public IppPrinters()
        {
        }

        public List<IppPrinter> Printers => printers;

        public void AddIppPrinter(IppPrinter printer) 
        { 
            printers.Add(printer);
        }

        public IEnumerator<IppPrinter> GetEnumerator()
        {
            return printers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
