using System.ComponentModel;
using System.Windows.Input;
using MicroMvvm;
using System.Windows;
using System.Runtime.CompilerServices;
using CsIppRequestLib;


namespace IppCheck
{
    public  class RequestWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private UserSettings us = UserSettings.GetInstance;
        private string m_sPrinterName;
        private IppAttributeCollection m_pac;
        private int m_iRequestNumber = 1;
        private string m_sPrinterIcon;

        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand); } }


        public RequestWindowViewModel() 
        { 
        }

        public Window MyWindow { get; set; }
        public string PrinterName 
        { 
            get => m_sPrinterName;
            set
            {
                m_sPrinterName = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Pac 
        /// 
        /// Collection of printer attributes returned when using
        /// the 'Attributes' button to query printer
        /// </summary>
        public IppAttributeCollection Pac
        {
            get => m_pac;
            set
            {
                m_pac = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// PrinterIcon
        /// 
        /// The icon for the selected printer
        /// </summary>
        public string PrinterIcon 
        { 
            get => m_sPrinterIcon;
            set
            {
                m_sPrinterIcon = value;
                OnNotifyPropertyChanged();
            }
        }

        public void OnLoaded(object sender, EventArgs e)
        {
            PrinterName = us.SelectedPrinter.QueryName;
            Pac = new IppAttributeCollection();
            ExecuteAttributesCommandAsync();
        }
        public void OnClose(object sender, EventArgs e)
        {
            MyWindow.Close();
        }

        /// <summary>
        /// ExecuteAttributesCommandAsync
        /// 
        /// Performs the Get-Printer-Attributes request and places the
        /// results in a collection of IppAttribute objets, Pac. This 
        /// method awaits the return of the IPP request.
        /// </summary>
        private async void ExecuteAttributesCommandAsync()
        {
            //Clear the existing observable collection
            Pac.Clear();
            m_iRequestNumber++;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                GetIppAttributesRequest gpa = new GetIppAttributesRequest(us.IppVersion, PrinterName, us.Ipps, m_iRequestNumber);
                CompletionStruct cs = await gpa.SendRequestAsync();
                if (IppHelpers.IsRequestSuccessful(cs.status))
                {
                    IEnumerator<IppAttribute> pas = gpa.GetAttributeValues();
                    while (pas.MoveNext())
                    {
                        IppAttribute pa = pas.Current as IppAttribute;
                        Pac.Add(pa);
                    }
                }
                else
                {
                    MessageBox.Show(string.Format("Get-Printer-Attributes request failed, reason: {0}", cs.status), Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                IppAttribute pi = Pac.Find("printer-icons");
                if (pi != null)
                {
                    PrinterIcon = pi.AttributeValues[0];
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
