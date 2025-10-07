using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IppCheck
{
    /// <summary>
    /// UserSettings
    /// 
    /// Singleton to provide user settings
    /// </summary>
    public sealed class UserSettings
    {
        private static readonly UserSettings instance = new UserSettings();
        private bool m_bJobSettingsAvailable;
        private bool m_bOutputcsvFileAvailable;
        private bool m_bPrintersFileAvailable;
        private bool m_bParametersFileAvailable;
        private bool m_bIpps;
        private string m_IppVersion;
        private string m_CustomTestPage;
        public IppPrinter SelectedPrinter {  get; set; }   

        static UserSettings()
        {
        }

        private UserSettings()
        {
        }

        public static UserSettings GetInstance
        {
            get
            {
                return instance;
            }
        }

        public bool JobSettingsAvailable { get => m_bJobSettingsAvailable; set => m_bJobSettingsAvailable = value; }
        public bool OutputcsvFileAvailable { get => m_bOutputcsvFileAvailable; set => m_bOutputcsvFileAvailable = value; }
        public bool PrintersFileAvailable { get => m_bPrintersFileAvailable; set => m_bPrintersFileAvailable = value; }
        public bool ParametersFileAvailable { get => m_bParametersFileAvailable; set => m_bParametersFileAvailable = value; }
        public bool Ipps { get => m_bIpps; set => m_bIpps = value; }
        public string IppVersion { get => m_IppVersion; set => m_IppVersion = value; }
        public string CustomTestPage { get => m_CustomTestPage; set => m_CustomTestPage = value; }
    }
}
