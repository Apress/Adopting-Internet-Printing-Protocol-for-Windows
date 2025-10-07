using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IppCheck
{
    public class PreferencesChangeEventArgs
    {
        private string m_sPrintersFile;
        private bool m_bOutputFile;
        private string m_sOutputFile;
        private string m_sParametersFile;
        private string m_sJobAttributesFile;
        public PreferencesChangeEventArgs(string sfile, string sparamsfile, string sjobattributesfile)
        {
            CreateOutputFile = false;
            OutputFile = string.Empty;
            PrintersFile = sfile;
            JobAttributesFile = sjobattributesfile;
            ParametersFile = sparamsfile;
        }

        public string PrintersFile 
        { 
            get => m_sPrintersFile; 
            set => m_sPrintersFile = value; 
        }
        public string OutputFile 
        { 
            get => m_sOutputFile; 
            set => m_sOutputFile = value; 
        }
        public bool CreateOutputFile 
        { 
            get => m_bOutputFile; 
            set => m_bOutputFile = value; 
        }
        public string ParametersFile 
        { 
            get => m_sParametersFile; 
            set => m_sParametersFile = value; 
        }
        public string JobAttributesFile 
        { 
            get => m_sJobAttributesFile; 
            set => m_sJobAttributesFile = value; 
        }
    }
}
