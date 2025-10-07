using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmLocalPrinters
{
    public class Logger
    {
        private readonly string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\RmLocalPrinters.log";
        private readonly bool logEnabled = false;

        public Logger(bool logEnabled)
        {
            this.logEnabled = logEnabled;
        }

        public void Log(string message)
        {

            if (this.logEnabled)
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                AppendLog(logEntry);
            }
        }


        private void AppendLog(string logEntry)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(logEntry);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while writing to the log: {ex.Message}");
            }
        }
    }
}
