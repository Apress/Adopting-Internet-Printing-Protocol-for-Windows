using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MicroMvvm;
using System.Windows;
using System.Reflection;
using System.Diagnostics;

namespace IppCheck
{
    public class RequestErrorWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string m_sErrorMessage = string.Empty;
        private string m_sippStatusCode;
        private string m_sippStatusCodeMessage;

        public RequestErrorWindowViewModel() 
        {
            ProcessError();
        }

        private void ProcessError()
        {
            ResponseErrorProperties _rep = ResponseErrorProperties.GetInstance;
            ErrorMessage = _rep.ResponseErrorMessage;
        }

        //Commands
        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand); } }

        public Window MyWindow { get; set; }
        public string ErrorMessage
        { 
            get => m_sErrorMessage;
            set
            {
                m_sErrorMessage = value;
                OnNotifyPropertyChanged();
            }
        }

        public string RequestIppStatusCode 
        { 
            get => m_sippStatusCode;
            set
            {
                m_sippStatusCode = value;
                OnNotifyPropertyChanged();
            }
        }

        public string IppStatusCodeMessage 
        { 
            get => m_sippStatusCodeMessage;
            set
            {
                m_sippStatusCodeMessage = value;
                OnNotifyPropertyChanged();
            }
        }

        public void OnLoaded(object sender, EventArgs e)
        {
            
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
