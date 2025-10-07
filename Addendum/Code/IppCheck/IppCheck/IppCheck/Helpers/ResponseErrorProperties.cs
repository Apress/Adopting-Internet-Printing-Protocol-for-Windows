
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IppCheck
{
    /// <summary>
    /// ResponseErrorProperties
    /// 
    /// Singleton class to acquire IPP response error codes between ViewModels
    /// </summary>
    public sealed class ResponseErrorProperties
    {
        private static readonly ResponseErrorProperties instance = new ResponseErrorProperties();
        //public IppStatusCode IppStatusCode { get; set; }
        public string ResponseErrorMessage { get; set; }  = string.Empty; 
        public string PrinterName { get; set; } = string.Empty;

        static ResponseErrorProperties()
        {

        }

        private ResponseErrorProperties()
        {

        }

        public static ResponseErrorProperties GetInstance
        {
            get
            {
                return instance;
            }
        }

    }
}
