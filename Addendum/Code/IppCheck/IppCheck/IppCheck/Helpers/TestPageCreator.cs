using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.StyledXmlParser.Jsoup.Nodes;
using System.IO;


namespace IppCheck
{
    public class TestPageCreator
    {
        private string m_tpPath;
        private iText.Layout.Document m_document = null;
        private iText.Layout.Element.Paragraph m_Parapgraph = null;
        private iText.Layout.Element.Paragraph m_SubParapgraph = null;
        private iText.Layout.Element.Paragraph m_PropertiesParapgraph = null;


        public TestPageCreator(string path) 
        {
            TestPagePath = path;
        }

        /// <summary>
        /// TestPagePath
        /// 
        /// The path and name of the test page to create
        /// </summary>
        public string TestPagePath 
        { 
            get => m_tpPath; 
            set => m_tpPath = value; 
        }

        private void Init()
        {
            // Must have write permissions to the path folder
            m_document = new iText.Layout.Document(new PdfDocument(new PdfWriter(TestPagePath)));
        }
        //AttributeItem
        public void MakeTestPage(IppPrinter printer, string image)
        {
            try
            {
                Init();
                CreateHeader("IPP Test Page");
                CreateSubHeader(printer.Name.Trim());
                AddImage(image);
                string user = Environment.UserName;
                string datetime = DateTime.Now.ToString();
                AddPrinterProperties(printer.Manufacturer, printer.IppVers, printer.Mopria, printer.Firmware, printer.Location, user, datetime);
                m_document.Close();
            }
            catch(Exception ex) 
            {
                throw new Exception("Failed to create test page, reason: " + ex.Message);
            }
        }

        private void CreateHeader(string headerText)
        {
            m_Parapgraph= new iText.Layout.Element.Paragraph(headerText).SetTextAlignment(TextAlignment.CENTER).SetFontSize(20);
            m_document.Add(m_Parapgraph);
        }

        private void CreateSubHeader(string subHeaderText) 
        {
            m_SubParapgraph = new iText.Layout.Element.Paragraph(subHeaderText).SetTextAlignment(TextAlignment.CENTER).SetFontSize(15);
            m_document.Add(m_SubParapgraph);
            LineSeparator ls = new LineSeparator(new SolidLine());
            m_document.Add(ls);
        }

        private void AddImage(string imagePath)
        {
            // Add image
            Image img = new Image(ImageDataFactory.Create(imagePath)).SetTextAlignment(TextAlignment.LEFT);
            m_document.Add(img);
        }
        
        private void AddPrinterProperties(string manufacturer, string ippVers, string mopriaCert, string firmware, string location, string user, string datetime)
        {
            string newText = string.Format("IPP Versions: {0}\n Mopria Certification: {1}\n Manufacturer: {2}\n Firmware: {3}\n Location: {4}\n User: {5}\n DateTime: {6}\n",
                ippVers, mopriaCert, manufacturer, firmware, location, user, datetime);
            m_PropertiesParapgraph = new iText.Layout.Element.Paragraph(newText).SetTextAlignment(TextAlignment.LEFT).SetFontSize(12);
            m_document.Add(m_PropertiesParapgraph);
        }

    }
}
