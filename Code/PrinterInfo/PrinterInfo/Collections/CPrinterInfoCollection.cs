using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PrinterInfo
{
    /// <summary>
    /// CPrinterInfoCollection
    /// 
    /// Collection of CPrinterInfo objects which hold information 
    /// about each printer to be queried.
    /// </summary>
    public class CPrinterInfoCollection : IEnumerable<CPrinterInfo>
    {
        private List<CPrinterInfo> m_List;

        public CPrinterInfoCollection()
        {
            m_List = new List<CPrinterInfo>();
        }

        public List<CPrinterInfo> PRINTER_INFO_COLLECTION
        {
            get { return m_List; }
            set { m_List = value; }
        }

        public IEnumerator<CPrinterInfo> GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Add(string s)
        {
            if (s.Length == 0)
                return;
            try
            {
                CPrinterInfo cpi = new CPrinterInfo(s.Trim());
                this.m_List.Add(cpi);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
