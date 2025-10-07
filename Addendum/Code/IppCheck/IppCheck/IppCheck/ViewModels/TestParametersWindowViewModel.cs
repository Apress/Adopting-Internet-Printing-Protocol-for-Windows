using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.Threading.Tasks;
using MicroMvvm;

namespace IppCheck
{
    public class TestParametersWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private List<string> m_lstOperationsSupported = null;
        ParamsFileManager _pfm = null;
        private string m_sIppMin;
        private string m_sIppMax;
        private string m_sMopriaVer;


        public TestParametersWindowViewModel()
        {
            OperationsSupported = new List<string>();
            _pfm = ParamsFileManager.GetInstance;
        }

        //Commands
        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand); } }

        public Window MyWindow { get; set; }
        /// <summary>
        /// Collection of operations supported by
        /// the printer.
        /// </summary>
        public List<string> OperationsSupported 
        { 
            get => m_lstOperationsSupported;
            set
            {
                m_lstOperationsSupported = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// IppMin
        /// 
        /// Minimum version of IPP supported
        /// </summary>
        public string IppMin 
        { 
            get => m_sIppMin;
            set
            {
                m_sIppMin = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// IppMax
        /// 
        /// Maximum version of Ipp supported
        /// </summary>
        public string IppMax 
        { 
            get => m_sIppMax;
            set
            {
                m_sIppMax = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// MopriaVer
        /// 
        /// Minimum version of Mopria supported
        /// </summary>
        public string MopriaVer 
        { 
            get => m_sMopriaVer;
            set
            {
                m_sMopriaVer = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// OnLoaded
        /// 
        /// Loa the interface with required minimal supported values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnLoaded(object sender, EventArgs e)
        {
            OperationsSupported = _pfm.SupportedOperations;
            IppMin = _pfm.IppVMin;
            IppMax = _pfm.IppVMax;
            MopriaVer = _pfm.MopriaCert;
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
