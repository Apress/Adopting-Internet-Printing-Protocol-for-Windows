using CsIppRequestLib;
using iText.Layout.Properties.Grid;
using Org.BouncyCastle.Crmf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IppCheck
{
    public class IppPrinter : INotifyPropertyChanged
    {
        private string m_sSelectedOperation;
        public string QueryName { get; set; }
        public string IppVers { get; set; }
        public string Mopria { get; set; }
        public bool Color { get; set; }
        public string Manufacturer { get; set; }
        public string Firmware { get; set; }
        public string Location { get; set; }
        public string IconUrl { get; set; }
        public string PrinterImage { get; set; }
        public string PrinterState { get; set; }
        public bool IppUsability { get; set; }
        public string ErrorString { get; set; }

        private PrinterIppStatus m_pis;

        public event PropertyChangedEventHandler? PropertyChanged;
        public required string Name { get; set; }

        ObservableCollection<string> m_lstOperationsSupported = new ObservableCollection<string>();

        private ObservableCollection<CsIppRequestLib.IppAttribute> attributes = new ObservableCollection<CsIppRequestLib.IppAttribute> ();

        public enum PrinterIppStatus
        {
            REACHABLE_CONFIGURED = 0,
            REACHABLE_NOT_CONFIGURED = 1,
            UNREACHABLE = 2
        };

        [SetsRequiredMembers]
        public IppPrinter(string name) 
        {
            QueryName = name;
            Name = name;    
        }

        /// <summary>
        /// AddAttribute
        /// 
        /// Add the printer attribute if it is unique
        /// </summary>
        /// <param name="attr"></param>
        public void AddAttribute(CsIppRequestLib.IppAttribute attr)
        {
            try 
            { 
                StringBuilder _sb = new StringBuilder();
                foreach (string val in attr.AttributeValues)
                {
                    _sb.Append(val + ",");
                }
                int len = _sb.Length;
                //remove the last ','
                _sb.Remove(len - 1, 1);

                if (Find(attr.Name) == null)
                {
                    processString(attr.Name, _sb.ToString());
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //
        public PrinterIppStatus Status
        {
            get => m_pis;
            set
            {
                m_pis = value;
                OnNotifyPropertyChanged();
            }
        }

        //

        /// <summary>
        /// Attributes 
        /// 
        /// List of Ipp Attributes
        /// </summary>
        public ObservableCollection<IppAttribute> Attributes 
        { 
            get => attributes;
            set
            {
                attributes = value;
            }
        }

        /// <summary>
        /// OperationsSupported 
        /// 
        /// These come back from get-printer-attributes as a comma-delimited string of enumerated values - i.e.
        /// they must be converted to string values for comparison. 
        /// 
        /// </summary>
        public ObservableCollection<string> OperationsSupported
        {
            get => m_lstOperationsSupported;
            set
            {
                m_lstOperationsSupported = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// SelectedOperation
        /// 
        /// This is the operation string selected by the user in the ListView "Ops Supported"
        /// Combobox.
        /// </summary>
        public string SelectedOperation 
        { 
            get => m_sSelectedOperation;
            set
            {
                m_sSelectedOperation = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// processString
        /// </summary>
        /// <param name="attString"></param>
        /// <param name="attValue"></param>
        private void processString(string attString, string attValue)
        {

            if (string.Compare(attString, "printer-name", true) == 0)
            {
                int pos = attValue.IndexOf(':');
                Name = attValue.Substring(pos + 1);
            }
            else if (string.Compare(attString, "print-color-mode-default", true) == 0)
            {
                int pos = attValue.IndexOf(':');
                string cv = attValue.Substring(pos + 1).Trim();
                if (cv == "color")
                {
                    Color = true;
                }
                else
                {
                    Color = false;
                }
            }
            else if (string.Compare(attString, "mopria-certified", true) == 0)
            {
                int pos = attValue.IndexOf(':');
                Mopria = attValue.Substring(pos + 1).Trim();
            }
            else if (string.Compare(attString, "printer-make-and-model", true) == 0)
            {
                int pos = attValue.IndexOf(':');
                string pmm = attValue.Substring(pos + 1).Trim();
                ProcessPrinterManufacturer(pmm);
            }
            else if (string.Compare(attString, "printer-firmware-string-version", true) == 0)
            {
                string[] strings = attValue.Split(new char[] { ',' });
                int num = strings.Length;
                for (int i = 0; i < num; i++)
                {
                    int pos = strings[i].IndexOf(':');
                    string fVer = strings[i].Substring(pos + 1).Trim();
                    if (i < (num - 1))
                        Firmware += fVer + " ";
                    else
                        Firmware += fVer;
                }
            }
            else if (string.Compare(attString, "printer-location", true) == 0)
            {
                int pos = attValue.IndexOf(':');
                Location = attValue.Substring(pos + 1);
            }
            else if (string.Compare(attString, "printer-state", true) == 0)
            {
                int pos = attValue.IndexOf(':');
                PrinterState = attValue.Substring(pos + 1);
            }
            else if (string.Compare(attString, "ipp-versions-supported", true) == 0)
            {
                string[] strings = attValue.Split(new char[] { ',' });
                int num = strings.Length;
                for (int i = 0; i < num; i++)
                {
                    int pos = strings[i].IndexOf(':');
                    string ippVer = strings[i].Substring(pos + 1).Trim();
                    if (i < (num - 1))
                        IppVers += ippVer + " ";
                    else
                        IppVers += ippVer;
                }
            }
            else if (string.Compare(attString, "printer-icons", true) == 0)
            {
                //Just take the first icon found
                string[] strings = attValue.Split(new char[] { ',' });
                int pos = strings[0].IndexOf(':');
                PrinterImage = strings[0].Substring(pos + 1).Trim();
            }

            else if(string.Compare(attString, "operations-supported", true) == 0)
            {
                string[] osc = attValue.Split(new char[] { ',' });
                foreach(string s in osc)
                {
                    OperationsSupported.Add(s);    
                }
            }

        }

        /// <summary>
        /// ProcessPrinterManufacturer
        /// 
        /// Recover the icon based on the manufacturer
        /// </summary>
        /// <param name="pmm"></param>
        private void ProcessPrinterManufacturer(string pmm)
        {
            try
            {
                if (pmm != null)
                {
                    string[] mis = pmm.Split(' ');
                    if (mis.Length > 0)
                    {
                        Manufacturer = mis[0].ToLower();
                        if (Manufacturer == "hp")
                            IconUrl = @"pack://application:,,,/IppCheck;component/Graphics/hp.png";
                        else if (Manufacturer == "lexmark")
                            IconUrl = @"pack://application:,,,/IppCheck;component/Graphics/lexmark.png";
                        else if (Manufacturer == "xerox")
                            IconUrl = @"pack://application:,,,/IppCheck;component/Graphics/xerox.png";
                        else if (Manufacturer == "brother")
                            IconUrl = @"pack://application:,,,/IppCheck;component/Graphics/brother.png";
                        else if (Manufacturer == "konica")
                            IconUrl = @"pack://application:,,,/IppCheck;component/Graphics/konica.png";
                        else if (Manufacturer == "epson")
                            IconUrl = @"pack://application:,,,/IppCheck;component/Graphics/epson.png";
                    }
                }
            }
            catch (Exception)
            {
                Manufacturer = @"pack://application:,,,/IppCheck;component/Graphics/ipp.png";
            }
        }

        /// <summary>
        /// IsOperationSupported
        /// 
        /// Compare operation in argument to the list of operations supported 
        /// returned from Get-Printer-Attributes request.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public bool IsOperationSupported(string operation)
        {
            return m_lstOperationsSupported.Any(s => string.Equals(s, operation, StringComparison.CurrentCultureIgnoreCase));
        }


        /// <summary>
        /// AccessIppUsability
        /// 
        /// Checks printer ipp and mopria verions for compliance against minumum
        /// versions accepted.
        /// </summary>
        /// <param name="dMinMopria"></param>
        /// <param name="dMinIpp"></param>
        public void AccessIppUsability(double dMinMopria, double dMinIpp)
        {
            try
            {
                if ((Mopria == null) || (IppVers == null))
                {
                    IppUsability = false;
                    ErrorString = $"{Name} failed IPP usability test, Mopria or IPP versioning not available";
                    return;
                }
                double dPrinterMopria = Convert.ToDouble(Mopria);
                string[] ippVers = IppVers.Split(' ');
                int num = ippVers.Length;
                double[] dIppVers = new double[num];

                for (int i = 0; i < num; i++)
                {
                    dIppVers[i] = Convert.ToDouble(ippVers[i]);
                }

                if (dPrinterMopria >= dMinMopria)
                {
                    for (int i = 0; i < num; i++)
                    {
                        if (dIppVers[i] >= dMinIpp)
                        {
                            IppUsability = true;
                            return;
                        }
                    }
                }

                IppUsability = false;
            }
            catch (Exception ex)
            {
                ErrorString = $"{Name} failed IPP usability test, reason: {ex.Message}";
                IppUsability = false;
            }
        }


        /// <summary>
        /// Find
        /// 
        /// Return a CsIppRequestLib.PrinterAttribute by attribute name.
        /// Returns null if not found
        /// </summary>
        /// <param name="attribute_name"></param>
        /// <returns></returns>
        public CsIppRequestLib.IppAttribute Find(string attribute_name)
        {
            return Attributes.FirstOrDefault(x => String.Equals(x.Name, attribute_name, StringComparison.CurrentCultureIgnoreCase));
        }

        private void OnNotifyPropertyChanged([CallerMemberName] string sPropertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }
    }
}
