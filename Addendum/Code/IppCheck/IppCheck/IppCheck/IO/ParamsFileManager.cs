using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;

namespace IppCheck
{
    /// <summary>
    /// Singleton class for Ipp Parameters
    /// </summary>
    public sealed class ParamsFileManager : IEnumerable<string>
    {
        private string m_sFile;
        private string m_sIppVMax;
        private string m_sIppVMin;
        private string m_sMopriaCert;
        private List<string> lstOpsSupportedCollection = new List<string>();

        private static readonly ParamsFileManager instance = new ParamsFileManager();
        private ParamsFileManager()
        {
        }

        static ParamsFileManager()
        {

        }

        public static ParamsFileManager GetInstance
        {
            get
            {
                return instance;
            }
        }

        public string ParamsFile { get => m_sFile; set => m_sFile = value; }
        public string IppVMax { get => m_sIppVMax; set => m_sIppVMax = value; }
        public string IppVMin { get => m_sIppVMin; set => m_sIppVMin = value; }
        public string MopriaCert { get => m_sMopriaCert; set => m_sMopriaCert = value; }
        public List<string> SupportedOperations => lstOpsSupportedCollection;

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<string> GetEnumerator()
        {
            return lstOpsSupportedCollection.GetEnumerator();
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// LoadXmlParamsFile
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void  LoadXmlParamsFile()
        {
            XDocument doc = null;
            if(ParamsFile.Length == 0)
            {
                throw new Exception("Must specify a parameters file to access!");
            }
            string xmlFile = ParamsFile;

            // Load the XML content
            try
            {
                doc = XDocument.Load(xmlFile);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing xml file: {ex.Message}");
            }

            try
            {
                //Ensure all the necessary columns exist
                foreach (var column in doc.Descendants("Printer"))
                {
                    var idElement = column.Element("IppAttributes");
                    {
                        if (idElement != null)
                        {
                            EnsureElementExists(idElement, "IppVersionMax");
                            EnsureElementExists(idElement, "IppVersionMin");
                            EnsureElementExists(idElement, "MopriaCertification");
                            EnsureElementExists(idElement, "OperationsSupported");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing IppTool parameters file: {xmlFile}, reason: " + ex.Message);
            }

            IppVMax = doc.Descendants("IppVersionMax").FirstOrDefault()?.Value;
            IppVMin = doc.Descendants("IppVersionMin").FirstOrDefault()?.Value;
            MopriaCert = doc.Descendants("MopriaCertification").FirstOrDefault()?.Value;

            try
            {
                lstOpsSupportedCollection = doc.Descendants("Operation")
                                                 .Select(op => op.Value)
                                                 .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing IppTool parameters file for Operations: {xmlFile}, reason: " + ex.Message);
            }
        }

        /// <summary>
        /// EnsureElementExists
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="elementName"></param>
        /// <exception cref="Exception"></exception>
        private void EnsureElementExists(XElement parent, string elementName)
        {
            if (parent.Element(elementName) == null)
            {
                throw new Exception($"Column: {elementName} does not exist");
            }
        }

        /// <summary>
        /// ContainsOperation
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public bool ContainsOperation(string op)
        {
            foreach (string sv in SupportedOperations)
            {
                if (string.Compare(sv, op, true) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
