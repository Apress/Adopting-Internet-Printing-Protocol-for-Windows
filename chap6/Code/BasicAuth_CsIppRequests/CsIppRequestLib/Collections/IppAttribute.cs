using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    /// <summary>
     /// IppAttribute
     /// 
     /// This class represents an IPP Attribute and can be used for jobs or printer
     /// attributes
     /// </summary>
    public class IppAttribute : IEnumerable<string>
    {
        private const byte bDateTime = 0x31;
        private string m_name = string.Empty;
        private List<string> m_lstValues = new List<string>();
        private byte m_tag;
        private int m_index;
        public IppAttribute(string n, string v, byte t) 
        {
            processNewAttribute(n, v, t);
        }

        public IppAttribute(string n, string v, byte t, int index)
        {
            processNewAttribute(n, v, t);
            Index = index;
        }

        /// <summary>
        /// processNewAttribute
        /// 
        /// Called by the constructor for all new IppAttributes. For printer attributes, this is called
        /// initially and new values are added via the AddAttributeValue method. For job attributes, this
        /// is called only once as AddAttributeValue is not needed for one to one relationships.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="v"></param>
        /// <param name="t"></param>
        /// <exception cref="Exception"></exception>
        private void processNewAttribute(string n, string v, byte t)
        {
            try
            {
                Name = n;
                Tag = t;
                m_lstValues.Add(v);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not process new attribute, reason: {ex.Message}");
            }
        }

        /// <summary>
        /// AddAttributeValue
        /// 
        /// Used when there can be a one to many relationship between attribute name
        /// and attribute value(s). Typically this would be used for printer attributes,
        /// where additional values to an existing attribute object are required.
        /// </summary>
        /// <param name="val"></param>
        public void AddAttributeValue(string val)
        {
            if (ContainsValue(val) == false)
            {
                m_lstValues.Add(val);
            }
        }

        /// <summary>
        /// Name
        /// 
        /// The attribute name
        /// </summary>
        public string Name { get => m_name; set => m_name = value; }

        public List<string> AttributeValues => m_lstValues;

        public byte Tag { get => m_tag; set => m_tag = value; }
        public int Index { get => m_index; set => m_index = value; }

        /// <summary>
        /// WriteAttributeValuesString
        /// 
        /// Write out the collection of multiple (or single) values as a comma-delimited string
        /// </summary>
        /// <returns></returns>
        public string WriteAttributeValuesString()
        {
            string valString = string.Empty;    
            int count = AttributeValues.Count;
            for (int i = 0; i < count; i++)
            {
                if( i == count - 1 )
                {
                    valString += AttributeValues[i];
                }
                else
                {
                    valString += AttributeValues[i] + ",";
                }
            }
            return valString;   
        }

        

        /// <summary>
        /// GetEnumerator
        /// 
        /// Public facing method to obtain an enumerator on the list 
        /// of attribute values. 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<string> GetEnumerator()
        {
            return m_lstValues.GetEnumerator();
        }

        /// <summary>
        /// IEnumerable.GetEnumerator
        /// 
        /// The private method.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// ContainsValue
        /// 
        /// Checks the array of attribute value strings
        /// for a match. Returns boolean result.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsValue(string value)
        {
            foreach(string sv in AttributeValues)
            {
                if(string.Compare(sv, value, true)  == 0)
                {
                    return true;    
                }
            }
            return false;
        }

    }
}
