using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MicroMvvm;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;

namespace IppCheck
{
    public class InformationWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string app_verison;
        private string sLibVersion;
        private string windows_version;
        private string service_pack;
        private string machine_info;
        private string bios_version;
        private string bios_release_date;

        public InformationWindowViewModel() 
        { 
        }

        //Commands
        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand); } }


        public Window MyWindow { get; set; }

        public string AppVersion 
        { 
            get => app_verison;
            set
            {
                app_verison = value;
                OnNotifyPropertyChanged();
            }
        }

        
        public string CsIppRequestLibVer
        { 
            get => sLibVersion;
            set
            {
				sLibVersion = value;
                OnNotifyPropertyChanged();
            }
        }
        

        public string WinVersion 
        { 
            get => windows_version;
            set
            {
                windows_version = value;
                OnNotifyPropertyChanged();
            }
        }
        public string ServicePack 
        { 
            get => service_pack;
            set
            {
                service_pack = value;
                OnNotifyPropertyChanged();
            }
        }

        public string MachineInfo
        { 
            get => machine_info;
            set
            {
                machine_info = value;
                OnNotifyPropertyChanged();
            }
        }
        public string BiosVersion 
        { 
            get => bios_version;
            set
            {
                bios_version = value;
                OnNotifyPropertyChanged();
            }
        }
        public string BiosReleaseDate 
        { 
            get => bios_release_date;
            set
            {
                bios_release_date = value;
                OnNotifyPropertyChanged();
            }
        }

        public void OnLoaded(object sender, EventArgs e)
        {
            GetInfo();
        }

        private void GetMachineInfo()
        {
            try
            {
                MachineInfo = RegSettings.GetLocalMachineRegistryStringValue(@"HARDWARE\DESCRIPTION\System\BIOS", "SystemProductName");
            }
            catch(Exception)
            {
                MachineInfo = "N/A";
            }
            try 
            {
                BiosVersion = RegSettings.GetLocalMachineRegistryStringValue(@"HARDWARE\DESCRIPTION\System\BIOS", "BIOSVersion");
            } 
            catch(Exception) 
            {
                BiosVersion = "N/A";
            }

            try
            {
                BiosReleaseDate = RegSettings.GetLocalMachineRegistryStringValue(@"HARDWARE\DESCRIPTION\System\BIOS", "BIOSReleaseDate");
            }
            catch (Exception)
            {
                BiosReleaseDate = "N/A";
            }

        }

        private void GetInfo()
        {
            try
            {
                AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                FileVersionInfo CsIppVersionInfo = FileVersionInfo.GetVersionInfo("CsIppRequestLib.dll");
                CsIppRequestLibVer = CsIppVersionInfo.FileVersion;
                OperatingSystem os = Environment.OSVersion;
                WinVersion = os.VersionString;
                ServicePack = os.ServicePack.ToString();
                GetMachineInfo();
            }
            catch(Exception ex) 
            {
                MessageBox.Show("Error retrieving application information, reason: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
