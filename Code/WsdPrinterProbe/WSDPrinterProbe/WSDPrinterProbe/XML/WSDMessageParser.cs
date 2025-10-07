using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WSDPrinterProbe
{
    public static class WSDMessageParser
    { 
        public static void ParseXmlMessage(XDocument xdoc)
        {
            var devices = xdoc.Descendants("ThisDevice");
            Console.WriteLine("fini");
        }

        static void EnsureElementExists(XElement parent, string elementName)
        {
            if (parent.Element(elementName) == null)
            {
                throw new Exception($"Column: {elementName} does not exist");
            }
        }
    }
}
