using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using CsIppRequestLib;
using MicroMvvm;
using System.IO;
using System;
using iText.StyledXmlParser.Jsoup.Helper;



namespace IppCheck
{
    public class MainWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public List<string> printers;
        private ObservableCollection<IppPrinter> ippPrinterCollection = null;
        public static event EventHandler<CompletedQueryEventArgs> CompletedQueryEvent;
        Dictionary<string, List<object>> jobAttributes = null;
        private string m_sPrintersList;
        private string m_sParamsFile;
        private string m_sNumPrintersLoaded;
        private string m_sTestPrinter = string.Empty;
        private string m_sOutputCsvFile = string.Empty;
        private bool m_bOutputCsvFile = false;
        private string m_sPrinterImage;
        private IppPrinter m_aiSelectedPrinter;
        private ResponseErrorProperties _rep;
        private ParamsFileManager _pfm;
        private bool m_bListAvailable;
        private bool m_bParamsFileLoaded;
        private bool m_bJobAttributesFileLoaded;
        private bool m_bShowTestButton;
        private bool m_bIPPs;
        private string m_version;
        private int m_iRequestNumber = 0;
        public UserSettings _us = null;


        //Commands
        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand); } }
        public ICommand TestCommand { get { return new RelayCommand(ExecuteQueryCommandAsync, EvaluateQueryCommand); } }
        public ICommand SetupCommand { get { return new RelayCommand(ExecuteSetupCommand); } }
        public ICommand TestPrintCommand { get { return new RelayCommand(ExecuteTestPrintCommandAsync, EvaluateTestPrintCommand); } }
        public ICommand ViewErrorCommand { get { return new RelayCommand(ExecuteViewErrorCommand, EvaluateViewErrorCommand); } }
        public ICommand CsvFileCommand { get { return new RelayCommand(ExecuteCsvFileCommandAsync, EvaluateCsvFileCommand); } }
        public ICommand ConnectCommand { get { return new RelayCommand(ExecuteConnectCommandAsync, EvaluateConnectCommand); } }
        public ICommand PrintersListViewCommand { get { return new RelayCommand(ExecuteCollectionCommand, EvaluateCollectionCommand); } }
        public ICommand TestParamsCommand { get { return new RelayCommand(ExecuteTestParamsCommand); } }
        public ICommand ValidateCommand { get { return new RelayCommand(ExecuteValidateCommand, EvaluateValidateCommand); } }
        public ICommand JobAttributesCommand { get { return new RelayCommand(ExecuteJobAttributesCommand); } }
          
        public ICommand PrinterAttributesCommand { get { return new RelayCommand(ExecutePrinterAttributesRequest, EvaluatePrinterAttributesRequest); } }
        public ICommand ConsumablesCommand { get { return new RelayCommand(ExecuteConsumablesRequest, EvaluateConsumablesRequest); } }
        public ICommand JobCreationCommand { get { return new RelayCommand(ExecuteJobCreationRequest, EvaluateJobCreationRequest); } }
        public ICommand IdentifyPrinterCommand { get { return new RelayCommand(ExecuteIdentifyPrinterRequest, EvaluateIdentifyPrinterRequest); } }



        public MainWindowViewModel()
        {
            _us = UserSettings.GetInstance;
            ippPrinterCollection = new ObservableCollection<IppPrinter>();  
            printers = new List<string>();
            
            if(RegSettings.TestForApplicationSubKey() ==false)
            {
                MessageBox.Show("Error - Could not create application sub key", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Versions = new ObservableCollection<string> { "1.0", "1.1", "2.0", "2.2" };
            Version = "1.0"; // Set default version
        }

        public Window MyWindow { get; set; }

        public ObservableCollection<IppPrinter> PrinterAttributesCollection
        {
            get => ippPrinterCollection;
            set
            {
                ippPrinterCollection = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Versions
        /// Collection of IPP versions available
        /// </summary>
        public ObservableCollection<string> Versions { get; set; }

        /// <summary>
        /// Version
        /// 
        /// The IPP version to test with - this gets sent with each request.
        /// </summary>
        public string Version
        {
            get => m_version;
            set
            {
                m_version = value;
                _us.IppVersion = value; 
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// NumPrintersLoaded 
        /// 
        /// This is the number of printers in the printer list
        /// that is loaded.
        /// </summary>
        public string NumPrintersLoaded 
        { 
            get => m_sNumPrintersLoaded;
            set
            {
                m_sNumPrintersLoaded = value;
                OnNotifyPropertyChanged();
            }
        }

        //The printer selected in the ListView by the user
        public IppPrinter SelectedPrinter 
        { 
            get => m_aiSelectedPrinter;
            set
            {
                if (value != null)
                {
                    m_aiSelectedPrinter = value;
                    if(value.PrinterImage != null)  
                        PrinterImage = value.PrinterImage;
                    else
                        PrinterImage = "pack://application:,,,/IppCheck;component/Graphics/baseprinter.png";
                    OnNotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// TestPrinter
        /// 
        /// Allows single use IPP Testing - overrides the 
        /// printer list. Bound to Text dependency.
        /// </summary>
        public string TestPrinter 
        { 
            get => m_sTestPrinter;
            set
            {
                m_sTestPrinter = value;
                UpdateTestStatus();
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// IPPS
        /// 
        /// Boolean to checkbox to indicate whether IPPS is to be used
        /// </summary>
        public bool IPPS
        {
            get => m_bIPPs;
            set
            {
                if (m_bIPPs != value)
                {
                    m_bIPPs = value;
                    _us.Ipps = value;
                    OnNotifyPropertyChanged();
                }
            }
        }


        /// <summary>
        /// OutputCsvFile
        /// 
        /// This is the filename chosen if the user elects to have test
        /// results output to a csv file.
        /// </summary>
        public string OutputCsvFile 
        { 
            get => m_sOutputCsvFile;
            set
            {
                m_sOutputCsvFile = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// CreateOutputCsvFile 
        /// 
        /// This bool is set if user wants to create an output csv file.
        /// Visibility of CSV button is dependent on this value. 
        /// </summary>
        public bool CreateOutputCsvFile 
        { 
            get => m_bOutputCsvFile;
            set
            {
                m_bOutputCsvFile = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// PrinterImage 
        /// 
        /// Printer image url (provided by IPP query)
        /// </summary>
        public string PrinterImage 
        { 
            get => m_sPrinterImage;
            set
            {
                m_sPrinterImage = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// ListAvailable
        /// 
        /// Boolean set true if number of printers in the printer list > 0.
        /// </summary>
        public bool ListAvailable 
        { 
            get => m_bListAvailable;
            set
            {
                m_bListAvailable = value;
                UpdateTestStatus();
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// ParamsFileLoaded 
        /// 
        /// Boolean to indicate whether the parameters (for test) file
        /// was found and loaded.
        /// </summary>
        public bool ParamsFileLoaded 
        { 
            get => m_bParamsFileLoaded;
            set
            {
                m_bParamsFileLoaded = value;
                OnNotifyPropertyChanged();
            }
        }

        public bool JobAttributesFileLoaded 
        { 
            get => m_bJobAttributesFileLoaded;
            set
            {
                m_bJobAttributesFileLoaded = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// ShowTestButton
        /// 
        /// This property is bound to the Test button visibility property.
        /// If either the ItemToTest or ListAvailable are true, this will be true.
        /// </summary>
        public bool ShowTestButton 
        { 
            get => m_bShowTestButton;
            set
            {
                m_bShowTestButton = value;
                OnNotifyPropertyChanged();
            }
        }

        private void UpdateTestStatus()
        {
            if((TestPrinter.Length > 5) || (ListAvailable == true))
            {
                ShowTestButton = true;
            }
            else
            {
                ShowTestButton = false; ;
            }
        }

        /// <summary>
        /// OnLoaded
        /// 
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnLoaded(object sender, EventArgs e)
        {
            try
            {
                SetupWindowViewModel.PreferencesChangeEvent += OnUserPreferencesChangeEvent;
                GetCurrentUserAppDefaults();
                LoadPrintersFromFile();
                ListAvailable = true;
            }
            catch (Exception ex)
            {
                ListAvailable = false;
            }

            try
            {
                m_sParamsFile = RegSettings.GetRegistryStringValue("ParamsFile");
                ParamsFileLoaded = true;
                _pfm = ParamsFileManager.GetInstance;
                _pfm.ParamsFile = m_sParamsFile;
                _pfm.LoadXmlParamsFile();
                _us.CustomTestPage = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CustomTestPage.pdf";
                _us.ParametersFileAvailable = true;
            }
            catch (Exception ex)
            {
                _us.ParametersFileAvailable = false;
                MessageBox.Show("Test parameters file missing: you cannot test until parameters are loaded.", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                ParamsFileLoaded = false;
            } 
        }

        /// <summary>
        /// GetCurrentUserAppDefaults
        /// 
        /// Retrieves current user settings 
        /// </summary>
        private void GetCurrentUserAppDefaults()
        {
          
            try
            {
                m_sPrintersList = RegSettings.GetRegistryStringValue("PrintersList");
                _us.PrintersFileAvailable = true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Unable to retrieve a printer list, reason: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                _us.PrintersFileAvailable = false;
            }

            try
            {
                OutputCsvFile = RegSettings.GetRegistryStringValue("OutputCsvFile");
                if ((RegSettings.GetRegistryStringValue("WriteOutputCsvFile") == "1") && (OutputCsvFile.Length > 0))
                {
                    _us.OutputcsvFileAvailable = true;
                    CreateOutputCsvFile = true;
                }
                else
                {
                    _us.OutputcsvFileAvailable = false;
                    CreateOutputCsvFile = false;
                }
            }
            catch (Exception)
            {
                _us.OutputcsvFileAvailable = false;
                CreateOutputCsvFile = false;
            }

            try
            {
                //Job Attributes
                string ja = RegSettings.GetRegistryStringValue("JobAttributesFile");
                jobAttributes = AttributeHelper.ProcessFile(ja);
                // Give the JobAttributeListEditor the name of the file that holds job attributes...
                JobAttributesListEditor _jale = JobAttributesListEditor.GetInstance;
                _jale.JobAttributesListFile = ja;
                _us.JobSettingsAvailable = true;
                JobAttributesFileLoaded = true;
            }
            catch(Exception ex)
            {
                JobAttributesFileLoaded = false;
                _us.JobSettingsAvailable = false;
            }
        }

        /// <summary>
        /// LoadPrintersFromFile
        /// 
        /// Method called to load the printers from the provided printer file into a Collection
        /// usable for this utility.
        /// </summary>
        private void LoadPrintersFromFile()
        {
            FileHelper.LoadPrintersFile(m_sPrintersList, ref printers);
            NumPrintersLoaded = "Number of printers to test: " + printers.Count.ToString();
            if (printers.Count == 0)
            {
                throw new Exception("No IPP printers on printer list..");
            }
        }

        /// <summary>
        /// OnUserPreferencesChangeEvent
        /// 
        /// This method is fired when the user changes the printers file in the 
        /// Setup Window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnUserPreferencesChangeEvent(object sender, PreferencesChangeEventArgs e)
        {
            if(e.PrintersFile != null)
            {
                //if a printers file was specified, load it..
                if (e.PrintersFile.Length > 0)
                {
                    m_sPrintersList = e.PrintersFile;
                    printers.Clear();
                    LoadPrintersFromFile();
                }
                CreateOutputCsvFile = e.CreateOutputFile;
                OutputCsvFile = e.OutputFile;
            }
        }

        private void OnQueryCompletedEvent(object sender, CompletedQueryEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        public void OnClose(object sender, EventArgs e)
        {
            MyWindow.Close();
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        /// <summary>
        /// EvaluateQueryCommand
        /// 
        /// See if a Query command is valid
        /// </summary>
        /// <returns></returns>
        private bool EvaluateQueryCommand()
        {
            if ((TestPrinter.Length > 0) || (printers.Count > 0)) 
            {
                CompletedQueryEvent += OnQueryCompletedEvent;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// ExecuteQueryCommand
        /// 
        /// Query the list of printers for IPP properties
        /// </summary>
        private async void ExecuteQueryCommandAsync()
        {
            double dMopria, dMinIpp, dMaxIpp;

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                dMopria = Convert.ToDouble(_pfm.MopriaCert);
            }
            catch(Exception)
            {
                dMopria = 0;
            }

            try
            {
                dMinIpp = Convert.ToDouble(_pfm.IppVMin);
            }
            catch(Exception)
            {
                dMinIpp = 0;
            }

            if (TestPrinter.Length == 0)
            {
                //Is a list of printers
                PrinterAttributesCollection.Clear();
                PrinterAttributesCollection = await Task.Run(() => (PrinterAttributeQuery.GetAllPrinterAttributesAsync(printers, IPPS, _us.IppVersion)));
                //check mopria/ipp compliance... 
                foreach (IppPrinter printer in ippPrinterCollection)
                {
                    try
                    {
                        printer.AccessIppUsability(dMopria, dMinIpp);
                        foreach (string ipp_op in _pfm.SupportedOperations)
                        {
                            //If any of the ipp ops are not found, then fail usability
                            if (printer.IsOperationSupported(ipp_op) == false)
                            {
                                printer.IppUsability = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        printer.ErrorString = "Error evaluating usability of this IPP printer";
                        printer.IppUsability = false;
                    }
                }
                
            }
            else
            {
                PrinterAttributesCollection = await Task.Run(() => (PrinterAttributeQuery.GetPrinterAttributesAsync(TestPrinter, IPPS, _us.IppVersion)));
              
                //check mopria/ipp compliance...
                foreach (IppPrinter printer in ippPrinterCollection)
                {
                    try
                    {
                        printer.AccessIppUsability(dMopria, dMinIpp);
                        //Now check IPP ops
                        foreach (string ipp_op in _pfm.SupportedOperations)
                        {
                            //If any of the ipp ops are not found, then fail usability
                            if (printer.IsOperationSupported(ipp_op) == false)
                            {
                                printer.IppUsability = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        printer.ErrorString = "Error evaluating usability of this IPP printer";
                        printer.IppUsability = false;
                    }
                }
                
                TestPrinter = string.Empty;
            }
            CompletedQueryEventArgs args = new CompletedQueryEventArgs(true);
            EventHandler<CompletedQueryEventArgs> handler = CompletedQueryEvent;
            if (handler != null)
            {
                handler(CompletedQueryEvent, args);
            }
            CompletedQueryEvent -= OnQueryCompletedEvent;
        }

        /// <summary>
        /// EvaluateTestPrintCommand
        /// 
        /// Only allow test print as an option if the printer is REACHABLE_CONFIGURED.
        /// </summary>
        /// <returns></returns>
        private bool EvaluateTestPrintCommand()
        {
            if (SelectedPrinter != null)
            {
                if (SelectedPrinter.Status == IppCheck.IppPrinter.PrinterIppStatus.REACHABLE_CONFIGURED)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// CreateCustomTestPage
        /// </summary>
        /// <param name="ai"></param>
        private async Task CreateCustomTestPage(IppPrinter ai)
        {
            TestPageCreator _tpc = new TestPageCreator(_us.CustomTestPage);
            string sImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Graphics", "baseprinter.png");
            _tpc.MakeTestPage(ai, sImagePath);
        }


        /// <summary>
        /// ExecuteTestPrintCommandAsync
        /// 
        /// For successful printer tests, this allows the user to print a custom test page.
        /// </summary>
        private async void ExecuteTestPrintCommandAsync()
        {
            if (SelectedPrinter != null)
            {
                try
                {
                    await CreateCustomTestPage(SelectedPrinter);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Creation of a custom test page for {SelectedPrinter.Name} failed, reason: {ex.Message}", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    string printer_name = SelectedPrinter.Name.Trim();
                    int retVal = await Task.Run(() => (PrinterAttributeQuery.PrintTestPageAsync(SelectedPrinter.QueryName, _us.CustomTestPage, jobAttributes)));
                    if (retVal == PrinterAttributeQuery.SUCCESS)
                    {
                        MessageBox.Show("A test pdf was sent to: " + printer_name, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("The process to send a test print failed, reason: " + retVal.ToString(), Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The process to send a test print failed, reason: {ex.Message}", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// EvaluateViewErrorCommand
        /// 
        /// Show View Error menu item if there is an error registered to the IppPrinter
        /// object.
        /// </summary>
        /// <returns></returns>
        private bool EvaluateViewErrorCommand()
        {
            if (SelectedPrinter != null) 
            {
                if(SelectedPrinter.ErrorString == null)
                    return false;
                else 
                    return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ExecuteViewErrorCommand
        /// 
        /// This method brings up the error dialog. This dialog shows what went wrong during the
        /// prnter IPP test.
        /// </summary>
        private void ExecuteViewErrorCommand()
        {
            try
            {
                string printer_name = SelectedPrinter.Name.Trim();
                _rep = ResponseErrorProperties.GetInstance;
                _rep.ResponseErrorMessage = SelectedPrinter.ErrorString;
                MessageBox.Show(_rep.ResponseErrorMessage, "IPP Conformance Error", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to display error on printer, reason: {ex.Message}", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
     

        private bool EvaluateCsvFileCommand()
        {
            if(ippPrinterCollection.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ExecuteCsvFileCommandAsync
        /// </summary>
        private async void ExecuteCsvFileCommandAsync()
        {
            try
            {
                await Task.Run(() => (FileHelper.WriteCsvFileAsync(OutputCsvFile, ippPrinterCollection)));
                MessageBox.Show("CSV output file created successfully!", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex) 
            {
                string sError = string.Format("The process to create the csv file {0} failed, reason: {1}", OutputCsvFile, ex.Message);
                MessageBox.Show(sError, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// EvaluateTestRequest
        /// 
        /// Support selected requests
        /// </summary>
        /// <returns></returns>
        private bool EvaluateTestRequest()
        {
            if ((SelectedPrinter != null) && (SelectedPrinter.SelectedOperation != null))
            {
                if((string.Compare(SelectedPrinter.SelectedOperation, "Validate-Job", true) == 0)||
                    (string.Compare(SelectedPrinter.SelectedOperation, "Print-Job", true) == 0) ||
                    (string.Compare(SelectedPrinter.SelectedOperation, "Create-Job", true) == 0) ||
                    (string.Compare(SelectedPrinter.SelectedOperation, "Send-Document", true) == 0) ||
                    (string.Compare(SelectedPrinter.SelectedOperation, "Identify-Printer", true) == 0))
                    {
                        return true;
                    }
            }
            return false;
        }

        /// <summary>
        /// ExecuteCsvFileCommandAsync
        /// </summary>
        private async void ExecuteTestRequest()
        {
            _us.SelectedPrinter = SelectedPrinter;
            var rw = new RequestWindow();
            rw.ShowDialog();
        }


        /// <summary>
        /// ExecuteSetupCommand
        /// 
        /// This method calls th setup dialog so the user can choose what file of printers 
        /// to use and whether to have a .csv file created.
        /// </summary>
        private void ExecuteSetupCommand()
        {
            var sw = new SetupWindow();
            sw.ShowDialog();
        }

        private bool EvaluateConnectCommand()
        {
            if (SelectedPrinter != null)
            {
                if (SelectedPrinter.Status == (IppCheck.IppPrinter.PrinterIppStatus.REACHABLE_CONFIGURED) || (SelectedPrinter.Status == IppCheck.IppPrinter.PrinterIppStatus.REACHABLE_NOT_CONFIGURED))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// ExecuteTestParamsCommand
        /// 
        /// Opens the a Windows where the test parameters can be
        /// viewed/modified. (TestParametersWindowViewModel)
        /// </summary>
        private void ExecuteTestParamsCommand()
        {
           var tpw = new TestParametersWindow();
           tpw.ShowDialog();
        }

        /// <summary>
        /// EvaluateValidateCommand
        /// </summary>
        /// <returns></returns>
        private bool EvaluateValidateCommand()
        {
            if (SelectedPrinter != null)
            {
                if (SelectedPrinter.ErrorString == null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// ExecuteValidateCommand
        /// 
        /// Validate the job attributes with the chosen IPP 
        /// printer
        /// </summary>
        private async void ExecuteValidateCommand()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                int request = 1;
                ValidateJobRequest vjr = new ValidateJobRequest("1.1", SelectedPrinter.QueryName, false, request, jobAttributes);
                CompletionStruct cs = await vjr.SendRequestAsync();
                MessageBox.Show(Status.GetIppStatusMessage(cs.status), Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    MessageBox.Show(ex.InnerException.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// ExecuteConnectCommand
        /// 
        /// Make an HTTP connection to the embedded web server of the printer.
        /// </summary>
        private async void ExecuteConnectCommandAsync()
        {
            try
            {
                string sp = SelectedPrinter.QueryName.Trim();
                await Task.Run(() => Connect.PrinterWebServerConnection(sp));
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ExecuteJobAttributesCommand
        /// 
        /// Windows to display job attributes provided from the xml file for subsequent 
        /// IPP ops.
        /// </summary>
        private void ExecuteJobAttributesCommand()
        {
            var jaw = new JobAttributesFileWindow();
            jaw.ShowDialog();
        }


        private bool EvaluateCollectionCommand()
        {
            if(ListAvailable == true)
                return true;
            else
                return false;
        }

        /// <summary>
        ///  ExecuteCollectionCommand
        ///  
        /// This method calls a dialog that allows the user to modify and possibly 
        /// alter the list of printers loaded from file. The alterations are not necessarily
        /// permanent unless the printers file is altered.
        /// </summary>
        private void ExecuteCollectionCommand()
        {
            PrinterListEditor ple = PrinterListEditor.GetInstance;
            ple.PrintersListFile = m_sPrintersList;
            ple.PrinterList = printers;
            var plw = new PrintersListWindow();
            plw.ShowDialog();
            try
            {
                printers = ple.PrinterList.ToList();
                NumPrintersLoaded = "Number of printers to test: " + printers.Count.ToString();
                if (printers.Count > 0)
                {
                    ListAvailable = true;
                }
                else
                {
                    ListAvailable = false;
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show("Error loading modified list, reason: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// EvaluatePrinterAttributesRequest
        /// </summary>
        /// <returns></returns>
        private bool EvaluatePrinterAttributesRequest()
        {
            if ((SelectedPrinter != null) && (SelectedPrinter.IppUsability == true)) 
            {
                    return true;
            }
            return false;
        }

        /// <summary>
        /// ExecutePrinterAttributesRequest
        /// </summary>
        private void ExecutePrinterAttributesRequest()
        {
            _us.IppVersion = Version;
            _us.SelectedPrinter = SelectedPrinter;
            var rw = new RequestWindow();
            rw.ShowDialog();
        }

        private bool EvaluateConsumablesRequest()
        {
            if ((SelectedPrinter != null) && (SelectedPrinter.IppUsability == true))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ExecuteConsumablesRequest
        /// </summary>
        private void ExecuteConsumablesRequest()
        {
            _us.IppVersion = Version;
            _us.SelectedPrinter = SelectedPrinter;
            var cw = new ConsumablesWindow();
            cw.ShowDialog();
        }

        private bool EvaluateJobCreationRequest()
        {
            if ((SelectedPrinter != null) && (SelectedPrinter.IppUsability == true))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ExecuteJobCreationRequest
        /// </summary>
        private void ExecuteJobCreationRequest()
        {
            _us.IppVersion = Version;
            _us.SelectedPrinter = SelectedPrinter;
            var jc = new JobCreationWindow();
            jc.ShowDialog();
        }

        private bool EvaluateIdentifyPrinterRequest()
        {
            if ((SelectedPrinter != null) && (SelectedPrinter.IppUsability == true))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ExecuteJobCreationRequest
        /// </summary>
        private async void ExecuteIdentifyPrinterRequest()
        {
            _us.SelectedPrinter = SelectedPrinter;
            try
            {
                m_iRequestNumber++;
                IdentifyPrinterRequest ipr = new IdentifyPrinterRequest(m_version, SelectedPrinter.Name, _us.Ipps, m_iRequestNumber);
                CompletionStruct cs = await ipr.SendRequestAsync();
                if (IppHelpers.IsRequestSuccessful(cs.status))
                {
                    MessageBox.Show($"Identify printer request to {SelectedPrinter.Name} was successful.", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(string.Format("Identify printer request failed, reason: {0}", cs.status), Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex)
            {

            }
          
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
