using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public class PrintJob
    {
        List<JobAttributeEntry> m_lstJobAttributes = new List<JobAttributeEntry>();

        public string[] jobAttributeNames = null;
        private string m_sFirstJobAttribute = string.Empty;
        private int m_iNumAttributes = 0;
       

        public PrintJob(string[] AttributeNames )
        {
            this.jobAttributeNames = AttributeNames;
            m_sFirstJobAttribute = jobAttributeNames[0];
            m_iNumAttributes = jobAttributeNames.Length;
        }

        public void AddJobAttributeEntry(string name, object value, byte tag)
        {
            JobAttributeEntry jae = new JobAttributeEntry(name, value, tag);
            m_lstJobAttributes.Add(jae);
        }
    }
}
