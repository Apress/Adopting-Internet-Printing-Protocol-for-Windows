using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IppCheck
{
    public static class Connect
    {
        /// <summary>
        /// PrinterWebServerConnection
        /// 
        /// Connects to printer web server
        /// </summary>
        /// <param name="hostname"></param>
        /// <exception cref="Exception"></exception>
        public static void PrinterWebServerConnection(string hostname)
        {
            string sUrl = "http://" + hostname;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(sUrl);
                psi.UseShellExecute = true;
                Process.Start(psi); 
            }
            catch (Exception ex)
            {
                throw new Exception("Error connecting to: " + hostname + ", reason: " + ex.Message);
            }
        }
    }
}
