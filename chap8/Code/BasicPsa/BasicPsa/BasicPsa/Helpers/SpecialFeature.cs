using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicPsa
{
    public class SpecialFeature
    {
        private string m_sName;
        private List<string> m_lstOptions = new List<string>() { "On", "Off" };
        private string m_sValue;
        public SpecialFeature(string name)
        {
            DisplayName = name;
        }

        public string DisplayName { get => m_sName; set => m_sName = value; }
        public List<string> Options { get => m_lstOptions; set => m_lstOptions = value; }
        public string Value { get => m_sValue; set => m_sValue = value; }
    }
}
