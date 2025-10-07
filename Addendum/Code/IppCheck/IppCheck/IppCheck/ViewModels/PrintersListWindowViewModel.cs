using MicroMvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IppCheck
{
    public class PrintersListWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public PrinterListEditor ple = PrinterListEditor.GetInstance;
        private List<string> printers = new List<string>();
        private string m_selectedPrinter;
        private string m_sNewPrinter;
        private bool m_bshowAddButton;
        private bool m_sListAlerted;

        public PrintersListWindowViewModel()
        {
            Printers = ple.PrinterList.ToList();
            ListAlerted = false;
        }

        //Commands
        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand); } }
        public ICommand AddPrinterCommand { get { return new RelayCommand(ExecuteAddPrinterCommand); } }
        public ICommand RemovePrinterCommand { get { return new RelayCommand(ExecuteRemovePrinterCommand, EvaluateRemovePrinterCommand); } }
        public ICommand CancelCommand { get { return new RelayCommand(ExecuteCancelCommand); } }

        public Window MyWindow { get; set; }
        public List<string> Printers 
        { 
            get => printers;
            set
            {
                printers = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// SelectedPrinter
        /// 
        /// Printer selected from the ListBox by the user.
        /// </summary>
        public string SelectedPrinter 
        { 
            get => m_selectedPrinter;
            set
            {
                m_selectedPrinter = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// NewPrinter
        /// 
        /// Printer to be added to printers list by the user
        /// </summary>
        public string NewPrinter 
        { 
            get => m_sNewPrinter;
            set
            {
                m_sNewPrinter = value;
                if (m_sNewPrinter.Length > 5)
                {
                    ShowAddButton = true;
                }
                else
                {
                    ShowAddButton = false;
                }
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// ShowAddButton 
        /// 
        /// Should the Add printer button be shown?
        /// </summary>
        public bool ShowAddButton 
        { 
            get => m_bshowAddButton;
            set
            {
                m_bshowAddButton = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        ///  ListAlerted
        ///  
        /// True is printer was added/removed from list...
        /// </summary>
        public bool ListAlerted 
        { 
            get => m_sListAlerted;
            set
            {
                m_sListAlerted = value;
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


        private void ExecuteAddPrinterCommand()
        {
            try
            {
                ple.Add(NewPrinter);
                Printers.Clear();
                Printers = ple.PrinterList.ToList();
                ListAlerted = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding printer: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool EvaluateRemovePrinterCommand()
        {
            if (SelectedPrinter == null)
                return false;
            else
                return true;
        }

        private void ExecuteRemovePrinterCommand()
        {
            try
            {
                ple.Remove(SelectedPrinter);
                Printers.Clear();
                Printers = ple.PrinterList.ToList();
                ListAlerted = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error removing printer: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ExecuteOverWriteCommandAsync
        /// 
        /// Write any new changes to file...
        /// </summary>
        private async void ExecuteOverWriteCommandAsync()
        {
            try
            {
                await Task.Run(() => (FileHelper.WritePrintersFileAsync(ple.PrintersListFile, ple.PrinterList)));
                MessageBox.Show("Printers file updated..", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                string sError = string.Format("Error overwriting printers file {0}, reason: {1}", ple.PrintersListFile, ex.Message);
                MessageBox.Show(sError, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool EvaluateExitCommand()
        {
            return true;
        }

        private void ExecuteExitCommand()
        {
            if (ListAlerted == true)
            {
                ExecuteOverWriteCommandAsync();
            }
            OnClose(this, null);
        }

        private void ExecuteCancelCommand()
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
