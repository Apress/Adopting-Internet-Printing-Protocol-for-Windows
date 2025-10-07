
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IppCheck
{
    /// <summary>
    /// PrinterListEditor
    /// 
    /// 
    /// Singleton to provide printer list editing capabilities
    /// </summary>
    public sealed class PrinterListEditor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string printersListFile;
        private static readonly PrinterListEditor instance = new PrinterListEditor();

        private List<string> printerList = new List<string>();

        static PrinterListEditor()
        {

        }

        private PrinterListEditor()
        {

        }

        public static PrinterListEditor GetInstance
        {
            get
            {
                return instance;
            }
        }

        public List<string> PrinterList 
        { 
            get => printerList;
            set
            {
                printerList = value;
                OnNotifyPropertyChanged();
            }
        }

        public string PrintersListFile 
        { 
            get => printersListFile;
            set
            {
                printersListFile = value;
                OnNotifyPropertyChanged();
            }
        }

        public void Remove(string printerName)
        {
            string s = Find(printerName);
            if (s != null)
            {
                PrinterList.Remove(s);
            }
        }

        public void Add(string printerName)
        {
            string s = Find(printerName);
            if(s == null)   
            {
                PrinterList.Add(printerName);   
            }
        }

        public string Find(string printer_name)
        {
            return PrinterList.FirstOrDefault(x => String.Equals(x, printer_name, StringComparison.CurrentCultureIgnoreCase));
        }

        private void OnNotifyPropertyChanged([CallerMemberName] string sPropertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }
    }
}
