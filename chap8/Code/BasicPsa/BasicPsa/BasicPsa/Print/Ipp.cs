using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Printers;

namespace BasicPsa
{
    static class Ipp
    {
        public static string atts = "hp-edge-to-edge-support,hp-enhanced-mixed-orientation-supported,hp-tabbed-printing-supported";

        public static async Task<IDictionary<string, IppAttributeValue>> GetVendorSpecialAttributesAsync(string printer_name)
        {
            IppPrintDevice printer = null;
            List<string> requested_attributes = new List<string>();

            try
            {
                requested_attributes = atts.Split(',').ToList<string>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error, could not recover requested attributes, reason: " + ex.Message);
            }
            try
            {
                printer = IppPrintDevice.FromPrinterName(printer_name);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error - unable to locate {printer_name} - ensure it is installed as an IPP printer. Reason for error: {ex.Message}");
            }

            if (printer != null)
            {
                try
                {
                    return await Task.Run(() => printer.GetPrinterAttributes(requested_attributes));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error on get-printer-attributes call: {ex.Message}");
                }
            }
            else
            {
                throw new Exception("Printer not found.");
            }
        }
    }
}
