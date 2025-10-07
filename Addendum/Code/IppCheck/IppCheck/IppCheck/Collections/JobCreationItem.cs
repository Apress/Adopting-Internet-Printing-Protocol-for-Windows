using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IppCheck
{
    public class JobCreationItem
    {
        private string m_sName;
        private string m_sValue;
        private string m_sDefault;
        public JobCreationItem(string n) 
        {
            Name = n;
            Value = string.Empty;
        }

        public string Name { get => m_sName; set => m_sName = value; }
        public string Value { get => m_sValue; set => m_sValue = value; }
        public string Default { get => m_sDefault; set => m_sDefault = value; }
    }
}
