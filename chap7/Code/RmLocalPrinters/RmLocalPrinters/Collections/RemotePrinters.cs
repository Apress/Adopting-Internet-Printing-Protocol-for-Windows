using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Collections
{

    public class RemotePrinters: IEnumerable<RemotePrinter>
    {
        private List<RemotePrinter> lstPrinters = new List<RemotePrinter>();
        public RemotePrinters()
        {
        }

        public List<RemotePrinter> LstPrinters => lstPrinters;

        /// <summary>
        /// AddRemotePrinter
        /// </summary>
        /// <param name="rp"></param>
        public void AddRemotePrinter(RemotePrinter rp)
        {
            if (rp != null)
            { 
                RemotePrinter p = Find(rp.Name);
                if (p == null)
                {
                    LstPrinters.Add(rp);
                }
            }
        }

        public void Clear()
        {
            LstPrinters.Clear();    
        }

        public IEnumerator<RemotePrinter> GetEnumerator()
        {
            return lstPrinters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public List<RemotePrinter> GetNonIppLocalPrinters()
        {
            List<RemotePrinter> lst = new List<RemotePrinter> ();
            foreach (RemotePrinter rp in LstPrinters)
            {
                if((rp.IsIppPrinter == false) && (rp.IsSoftwareOnlyPrinter == false))
                {
                    lst.Add(rp);    
                }
            }
            return lst;
        }

        public RemotePrinter Find(string name)
        {
            return LstPrinters.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
