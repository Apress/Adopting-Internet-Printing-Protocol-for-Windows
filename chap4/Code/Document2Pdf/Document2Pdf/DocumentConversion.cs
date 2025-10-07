using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPPUtil
{
    /// <summary>
    /// Singleton class for information on conversion of MS Office docs to pdf
    /// </summary>
    public sealed class DocumentConversion
    {
        public enum DOC_CONVERSION_ENGINE
        {
            NONE = 0,
            MS_OFFICE = 1,
            SPIRE = 2
        };

        public DOC_CONVERSION_ENGINE engineUsed;    

        private static readonly DocumentConversion instance = new DocumentConversion();

        public string _document;
        public string _pdfdocument;
        public bool _success;
        public string _message;

        static DocumentConversion(){}

        private DocumentConversion(){}

        public static DocumentConversion GetInstance
        {
            get
            {
                return instance;
            }
        }
    }
}
