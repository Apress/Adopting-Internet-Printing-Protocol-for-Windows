using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Collections
{
    public class RemotePrinter
    {
        private string m_sName;
        private string m_sStatus;
        private string m_sPortName;
        private string m_sDriverName;
        private string m_sLocation;
        private bool m_bLocal;
        private bool m_bIsSoftwareOnlyPrinter;
        private bool m_bIsIppPrinter;
        

        public RemotePrinter(ManagementObject printer)
        {
            Name = printer["Name"].ToString();
            DriverName = printer["DriverName"].ToString();
            PortName = printer["PortName"].ToString();


            if (printer["Status"] != null)
                Status = printer["Status"].ToString();
            else
                Status = "N/A";


            if (printer["Location"] != null)
                Location = printer["Location"].ToString();
            else
                Location = "N/A";

            if (printer["Local"] != null)
                Local = Convert.ToBoolean(printer["Local"]);
            else
                Local = false;

            ProcessPrinterEntry(PortName, DriverName);
        }

        public string Name { get => m_sName; set => m_sName = value; }
        public string Status { get => m_sStatus; set => m_sStatus = value; }
        public string PortName { get => m_sPortName; set => m_sPortName = value; }
        public string DriverName { get => m_sDriverName; set => m_sDriverName = value; }
        public string Location { get => m_sLocation; set => m_sLocation = value; }
        public bool Local { get => m_bLocal; set => m_bLocal = value; }
        public bool IsSoftwareOnlyPrinter { get => m_bIsSoftwareOnlyPrinter; set => m_bIsSoftwareOnlyPrinter = value; }
        public bool IsIppPrinter { get => m_bIsIppPrinter; set => m_bIsIppPrinter = value; }

        private void ProcessPrinterEntry(string port_name, string driver_name)
        {
            if((string.Compare(port_name, "nul:", true) == 0) ||
                    (string.Compare(port_name, "PORTPROMPT:", true) == 0) ||
                   (string.Compare(port_name, "FILE:", true) == 0) ||
                   (string.Compare(port_name, "SHRFAX:", true) == 0))
                   {
                        IsSoftwareOnlyPrinter = true;   
                   }
            else
            {
                IsSoftwareOnlyPrinter = false;
            }

            if(string.Compare(driver_name, "Microsoft IPP Class Driver", true) == 0)
            {
                IsIppPrinter = true;    
            }
            else
            {
                IsIppPrinter = false;  
            }
        }               
    }
}
