using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MicroMvvm;
using System.Runtime.CompilerServices;

namespace CS_GPA_CH3_WPF
{
    public class MainWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private PrintMarkerCollection _pmc = new PrintMarkerCollection();
        PrinterAttributes _pas = new PrinterAttributes();
        private string m_sPrinterPicUri;
        private string m_sFirmware;

        //Commands
        public ICommand GetCommand { get { return new RelayCommand(ExecuteGetCommand, EvaluateGetCommand); } }
        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand) ; } }


        public Window MyWindow { get; set; }

        public void OnLoaded(object sender, EventArgs e)
        {

        }

        public PrintMarkerCollection Pmc
        {
            get => _pmc;
            set
            {
                _pmc = value;
                OnNotifyPropertyChanged();
            }
        }

        public string Printer
        {
            get;
            set;
        }

        public string Firmware
        {
            get => m_sFirmware;
            set
            {
                m_sFirmware = value;
                OnNotifyPropertyChanged();
            }
        }
        public string PrinterPicUri 
        { 
            get => m_sPrinterPicUri;
            set
            {
                m_sPrinterPicUri = value;
                OnNotifyPropertyChanged();
            }
        }

        private void GetMarkerLevels(MarkerProperties _props)
        {
            int num = _props.lstMarkerNames.Count;

            for (int i = 0; i < num; i++)
            {
                string name = _props.lstMarkerNames[i];
                uint cl = _props.lstMarkerCurrentLevels[i];
                uint hl = _props.lstMarkerHighLevels[i];
                uint ll = _props.lstMarkerLowLevels[i];
                PrintMarker pm = new PrintMarker(name, cl, hl, ll);
                Pmc.AddMarker(pm);
            }
        }

        /// <summary>
        /// GetPrinterAttributes
        /// </summary>
        private async void GetPrinterAttributes()
        {

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _pas = await WinIppPinvokeMethods._getPrinterAttributes(Printer, Environment.UserName, (uint)Defines.HTTP_ENCRYPTION.HTTP_ENCRYPTION_IF_REQUESTED);
                //get the marker attributes...
                MarkerProperties _props = MarkerProperties.GetInstance;
                _props.Clear();
                List<string> pIcons = _pas.GetAttributeValues("printer-icons");
                PrinterPicUri = pIcons[0];
                List<string> sFirmwareVers = _pas.GetAttributeValues("printer-firmware-string-version");
                Firmware = sFirmwareVers[0];
                _props.FillMarkerCollections("marker-low-levels", _pas.GetAttributeValues("marker-low-levels"));
                _props.FillMarkerCollections("marker-high-levels", _pas.GetAttributeValues("marker-high-levels"));
                _props.FillMarkerCollections("marker-levels", _pas.GetAttributeValues("marker-levels"));
                _props.FillMarkerCollections("marker-names", _pas.GetAttributeValues("marker-names"));
                GetMarkerLevels(_props);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"GetPrinterAttributes failed for {Printer}, reason: {ex.Message}");
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
            
        }

        /// <summary>
        ///  EvaluateGetCommand
        /// </summary>
        /// <returns></returns>
        private bool EvaluateGetCommand()
        {
            if (Printer == null)
            {
                return false;
            }
            else
            {
                return Printer.Length > 0 ? true : false;
            }
        }

        /// <summary>
        /// ExecuteGetCommand
        /// </summary>
        private void ExecuteGetCommand()
        {
            GetPrinterAttributes();
        }

        public void OnClose(object sender, EventArgs e)
        {
            MyWindow.Close();
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private bool EvaluateExitCommand()
        {
            return true;
        }

        private void ExecuteExitCommand()
        {
            //Save any application setting..
            OnClose(this, null);
        }

        // <summary>
        /// OnNotifyPropertyChanged
        /// </summary>
        /// <param name="sPropertyName"></param>
        private void OnNotifyPropertyChanged([CallerMemberName] string sPropertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }


    }
}
