using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MicroMvvm;
using System.Windows;
using System.Runtime.CompilerServices;
using CsIppRequestLib;

namespace IppCheck
{
    public class ConsumablesWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private UserSettings us = UserSettings.GetInstance;
        private string m_sPrinterName;
        private int m_iRequestNumber = 0;
        private List<uint> lstMarkerLowLevels = new List<uint>();
        private List<uint> lstMarkerHighLevels = new List<uint>();
        private List<uint> lstMarkerCurrentLevels = new List<uint>();
        private List<string> lstMarkerNames = new List<string>();
        private PrintMarkerCollection _pmc = new PrintMarkerCollection();

        //Commands
        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand); } }

        public ConsumablesWindowViewModel()
        {

        }

        public Window MyWindow { get; set; }

        public PrintMarkerCollection Pmc
        {
            get => _pmc;
            set
            {
                _pmc = value;
                OnNotifyPropertyChanged();
            }
        }


        public string PrinterName
        {
            get => m_sPrinterName;
            set
            {
                m_sPrinterName = value;
                OnNotifyPropertyChanged();
            }
        }

        public void OnLoaded(object sender, EventArgs e)
        {
            PrinterName = us.SelectedPrinter.QueryName;
            ExecuteAttributesCommandAsync();
        }

        private async void ExecuteAttributesCommandAsync()
        {
            m_iRequestNumber++;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                GetIppAttributesRequest gpa = new GetIppAttributesRequest(us.IppVersion, PrinterName, us.Ipps, m_iRequestNumber);
                CompletionStruct cs = await gpa.SendRequestAsync();
                if (IppHelpers.IsRequestSuccessful(cs.status))
                {
                    try
                    {
                        await Task.Run(() => ExtractMarkerInfo(ref gpa));
                    }
                    catch(Exception exml)
                    {
                        MessageBox.Show(string.Format("Extraction of marker levels failed, reason: {0}", exml.Message), Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show(string.Format("Get-Printer-Attributes request failed, reason: {0}", cs.status), Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// ClearMarkerCollections
        /// </summary>
        private void ClearMarkerCollections()
        {
            lstMarkerCurrentLevels.Clear();
            lstMarkerNames.Clear();
            lstMarkerLowLevels.Clear();
            lstMarkerHighLevels.Clear();
        }
        /// <summary>
        /// ExtractMarkerInfo
        /// </summary>
        /// <param name="gpa"></param>
        private void ExtractMarkerInfo(ref GetIppAttributesRequest gpa)
        {

            ClearMarkerCollections();
            IppAttribute pmns = gpa.Pac.Find("marker-names");
            if (pmns != null)
            {
                string[] mns = pmns.WriteAttributeValuesString().Split(",");
                foreach (string mn in mns)
                {
                    lstMarkerNames.Add(mn);
                }
            }

            IppAttribute pmls = gpa.Pac.Find("marker-levels");
            if (pmls != null)
            {
                string[] mls = pmls.WriteAttributeValuesString().Split(",");
                foreach (string ml in mls)
                {
                    lstMarkerCurrentLevels.Add(Convert.ToUInt32(ml));
                }
            }

            IppAttribute pmll = gpa.Pac.Find("marker-low-levels");
            if (pmll != null)
            {
                string[] mll = pmll.WriteAttributeValuesString().Split(",");
                foreach (string ll in mll)
                {
                    lstMarkerLowLevels.Add(Convert.ToUInt32(ll));
                }
            }

            IppAttribute pmhl = gpa.Pac.Find("marker-high-levels");
            if (pmhl != null)
            {
                string[] mhl = pmhl.WriteAttributeValuesString().Split(",");
                foreach (string hl in mhl)
                {
                    lstMarkerHighLevels.Add(Convert.ToUInt32(hl));
                }
            }

            int num = lstMarkerNames.Count; 
            for (int i = 0; i < num; i++)
            {
                string name = lstMarkerNames[i];
                uint cl = lstMarkerCurrentLevels[i];
                uint hl = lstMarkerHighLevels[i];
                uint ll = lstMarkerLowLevels[i];
                PrintMarker pm = new PrintMarker(name, cl, hl, ll);
                Pmc.AddMarker(pm);
            }
            return;
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
            OnClose(this, null);
        }

        /// <summary>
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
