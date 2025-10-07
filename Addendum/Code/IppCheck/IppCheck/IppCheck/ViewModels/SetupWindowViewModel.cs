using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using MicroMvvm;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Reflection;

namespace IppCheck
{
    public class SetupWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string m_sprintersfile;
        private String m_sparamsfile;
        private bool m_bCbOutputFile;
        private string m_sOutputFile;
        private string m_sJobAttributesFile;
        private UserSettings _us = UserSettings.GetInstance;

        public SetupWindowViewModel() 
        {
            PrintersFile = string.Empty;
        }

        //-----Events available for subscription-----
        public static event EventHandler<PreferencesChangeEventArgs> PreferencesChangeEvent;    

        //Commands
        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand); } }
        public ICommand CancelCommand { get { return new RelayCommand(ExecuteCancelCommand); } }
        public ICommand FileCommand { get { return new RelayCommand(ExecuteFileCommand); } }
        public ICommand InfoCommand { get { return new RelayCommand(ExecuteInfoCommand); } }
        public ICommand OutputFileCommand { get { return new RelayCommand(ExecuteOutputFileCommand); } }
        public ICommand ParametersFileCommand { get { return new RelayCommand(ExecuteParamsFileCommand); } }
        public ICommand JobAttributesFileCommand { get { return new RelayCommand(ExecuteJobAttributesFileCommand); } }

        public Window MyWindow { get; set; }
        

        /// <summary>
        /// PrintersFile
        /// Bound to the Printers File Path textbox.
        /// </summary>
        public string PrintersFile 
        { 
            get => m_sprintersfile;
            set
            {
                m_sprintersfile = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// ParamsFile
        /// The xml file holding test parameters
        /// </summary>
        public string ParamsFile 
        { 
            get => m_sparamsfile;
            set
            {
                m_sparamsfile = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// CbOutputFile 
        /// 
        /// Determines if an output file will be used for results.
        /// </summary>
        public bool CbOutputFile 
        { 
            get => m_bCbOutputFile;
            set
            {
                m_bCbOutputFile = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// OutputFile 
        /// 
        /// Bound to TextBox OuputFile dependency. Sets file where multiple
        /// print tests can be recorded.
        /// </summary>
        public string OutputFile 
        { 
            get => m_sOutputFile;
            set
            {
                m_sOutputFile = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// JobAttributesFile
        /// 
        /// File holding job attributes desired for print
        /// </summary>
        public string JobAttributesFile 
        { 
            get => m_sJobAttributesFile;
            set
            {
                m_sJobAttributesFile = value;
                OnNotifyPropertyChanged();
            }
        }

        public void OnLoaded(object sender, EventArgs e)
        {
            GetCurrentUserAppDefaults();
        }

        /// <summary>
        /// OnFileDialog
        /// </summary>
        public string OnFileDialog(bool createFile, string filterString)
        {
            if (createFile == false)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = filterString;
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 
                openFileDialog.AddExtension = true;
                openFileDialog.CheckFileExists = true;
                openFileDialog.CheckPathExists = true;

                try
                {
                    if (openFileDialog.ShowDialog() == true)
                    {
                        return openFileDialog.FileName;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    string errorString = string.Format("Error retrieving default file path, reason: {0}", ex.Message);
                    MessageBox.Show(errorString, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                    return null;
                }
               
            }
            else
            {
                try
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = filterString;
                    sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    sfd.CheckFileExists = false;
                    sfd.AddExtension = true;

                    if (sfd.ShowDialog() == true)
                    {
                        return sfd.FileName;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch(Exception ex) 
                {
                    string errorString = string.Format("Error retrieving file path, reason: {0}", ex.Message);
                    MessageBox.Show(errorString, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                    return null;
                }
            }
        }

        /// <summary>
        /// SaveUserSettings
        /// 
        /// When user updates settings, save these to registry...
        /// </summary>
        private void SaveUserSettings()
        {
            if((CbOutputFile == true) && (OutputFile == null))
            {
                MessageBox.Show("The CSV output file checkbox was enabled, but no output file was specified.", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if(OutputFile == null)
            {
                CbOutputFile = false;
            }
            try 
            {
                RegSettings.SetRegistryStringValue("PrintersList", PrintersFile);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to save printers list to registry, reason: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                if(ParamsFile.Length > 0)
                    RegSettings.SetRegistryStringValue("ParamsFile", ParamsFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save test parameters file to registry, reason: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                if(JobAttributesFile.Length > 0)
                    RegSettings.SetRegistryStringValue("JobAttributesFile", JobAttributesFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save job attributes file to registry, reason: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                
                if (OutputFile != null)
                {
                    RegSettings.SetRegistryStringValue("OutputCsvFile", OutputFile);
                    if (CbOutputFile == true)
                        RegSettings.SetRegistryStringValue("WriteOutputCsvFile", "1");
                    else
                        RegSettings.SetRegistryStringValue("WriteOutputCsvFile", "0");
                }
                else
                {
                    RegSettings.SetRegistryStringValue("WriteOutputCsvFile", "0");
                }

                //Tell the subscripber that we've changes the printers file...
                PreferencesChangeEventArgs args = new PreferencesChangeEventArgs(PrintersFile, JobAttributesFile, ParamsFile);
                args.CreateOutputFile = CbOutputFile;
                if (CbOutputFile == true)
                {
                    args.OutputFile = OutputFile;
                }
                OnUserSettingsChange(args);
                MessageBox.Show("Settings Saved", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save settings, reason: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// OnUserSettingsChange
        /// 
        /// Fires user setting change notification event(s)
        /// </summary>
        /// <param name="e"></param>
        private static void OnUserSettingsChange(PreferencesChangeEventArgs e)
        {
            EventHandler<PreferencesChangeEventArgs> handler = PreferencesChangeEvent;
            if(handler != null)
            {
                handler(SetupWindowViewModel.PreferencesChangeEvent, e);
            }
        }

        /// <summary>
        /// GetCurrentUserAppDefaults
        /// 
        /// Retrieves current user application settings from HKCU\Software\IppCheck
        /// </summary>
        private void GetCurrentUserAppDefaults()
        {
            CbOutputFile = false;

            try
            {
                ParamsFile = RegSettings.GetRegistryStringValue("ParamsFile");
                _us.ParametersFileAvailable = true;
            }
            catch (Exception ex)
            {
                _us.ParametersFileAvailable = false;
            }

            try
            {
                PrintersFile = RegSettings.GetRegistryStringValue("PrintersList");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Notice: printers list or output csv file not specified: {ex.Message}", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            try
            {
                _us.OutputcsvFileAvailable = false;
                OutputFile = RegSettings.GetRegistryStringValue("OutputCsvFile");
                if ((RegSettings.GetRegistryStringValue("WriteOutputCsvFile") == "1") && (OutputFile.Length > 0))
                {
                    CbOutputFile = true;
                    _us.OutputcsvFileAvailable = true;
                }
            }
            catch(Exception ex)
            {
                _us.OutputcsvFileAvailable = false;
            }
               
            try
            {
                JobAttributesFile = RegSettings.GetRegistryStringValue("JobAttributesFile");
                _us.JobSettingsAvailable = true;
            }
            catch(Exception ex)
            {
                _us.JobSettingsAvailable = false;
            }
        }
        public void OnClose(object sender, EventArgs e)
        {
            MyWindow.Close();
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void ExecuteFileCommand()
        {
            PrintersFile = OnFileDialog(true, "TXT files (*.txt)|*.txt");
            if(FileHelper.TestPrintersFile(PrintersFile) == false)
            {
                MessageBox.Show(PrintersFile + " could not be created or is not valid...", Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                PrintersFile = string.Empty;
            }
        }

        private void ExecuteParamsFileCommand()
        {
            ParamsFile = OnFileDialog(true, "XML files (*.xml)|*.xml");
        }
        /// <summary>
        /// ExecuteOutputFileCommand
        /// </summary>
        private void ExecuteOutputFileCommand()
        {
            OutputFile = OnFileDialog(true, "CSV files (*.csv)|*.csv");
        }

        private void ExecuteJobAttributesFileCommand()
        {
            JobAttributesFile = OnFileDialog(true, "TXT files (*.txt)|*.txt");
        }

        private void ExecuteCancelCommand()
        {
            OnClose(this, null);
        }

        private void ExecuteInfoCommand()
        {
            var iw = new InformationWindow();
            iw.Show();
        }

        private bool EvaluateExitCommand()
        {
            return true;
        }

        private void ExecuteExitCommand()
        {
            SaveUserSettings();
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
