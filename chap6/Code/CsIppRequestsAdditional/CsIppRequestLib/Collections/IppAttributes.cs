using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public class IppAttributes : IEnumerable<IppAttribute>

    {
        List<IppAttribute> m_lstAttributes = new List<IppAttribute>();
        public IppAttributes() { }

        public List<IppAttribute> AttributesList => m_lstAttributes;

        public void AddAttribute(IppAttribute attr)
        {
            try
            {
                IppAttribute pa = Find(attr.Name);
                if (pa == null)
                {
                    AttributesList.Add(attr);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not add attribute, reason: {ex.Message}");
            }
        }

        public void AddJobAttribute(IppAttribute attr)
        {
            try
            {
                AttributesList.Add(attr);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not add job attribute, reason: {ex.Message}");
            }
        }


        public IEnumerator<IppAttribute> GetEnumerator()
        {
            return m_lstAttributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Find
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IppAttribute Find(string name)
        {
            return m_lstAttributes.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Find
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool IsSupported(string name, string value)
        {
            IppAttribute pa = Find(name);
            if(pa == null) 
            { 
                return false; 
            }
            else
            {
                return pa.ContainsValue(value);
            }
        }


        /// <summary>
        /// GetAttributeValues
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<string> GetAttributeValues(string name)
        {
            IppAttribute pa = Find(name);
            if (pa == null)
                return null;
            else
            {
                return pa.AttributeValues;
            }
        }


    }
}
