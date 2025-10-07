using IPPUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Document2Pdf
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string Document = @"C:\Temp\TestWordDoc.docx";
            DocumentConversion _dc = DocumentConversion.GetInstance;
            _dc.engineUsed = DocumentConversion.DOC_CONVERSION_ENGINE.MS_OFFICE;
            _dc._document = Document;
            try
            {
                if (_dc.engineUsed == DocumentConversion.DOC_CONVERSION_ENGINE.MS_OFFICE)
                    _dc._pdfdocument = ComOfficeConversion.Convert2Pdf(_dc._document);
                else
                    _dc._pdfdocument = SpireOfficeConversion.Convert2Pdf(_dc._document);
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"Document conversion failed, reason: {ex.Message}");
            }
        }
    }
}
