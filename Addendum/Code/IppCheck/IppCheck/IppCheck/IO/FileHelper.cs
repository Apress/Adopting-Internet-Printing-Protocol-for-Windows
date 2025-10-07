using System.IO;
using System.Collections.ObjectModel;
using CsIppRequestLib;

namespace IppCheck
{
    public class FileHelper
    {
        /// <summary>
        /// LoadPrintersFile
        /// 
        /// Loads all the printers in the file (one per line) and puts them
        /// into a collection of printer strings.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="printers"></param>
        /// <exception cref="Exception"></exception>
        public static void LoadPrintersFile(string path, ref List<string> printers) 
        {
            try
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines) 
                { 
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        printers.Add(line.Trim());
                    }
                }
            }
            catch (Exception ex) 
            {
                throw new Exception(string.Format("Unable to retrieving file: {0} of printers to test, reason: {1}", path, ex.Message));
            }
        }

        /// <summary>
        /// TestPrintersFile
        /// 
        /// Tests if a file exists or not. If the file does not exist, attempts to open
        /// (and then close) said file. Returns boolean result.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool TestPrintersFile(string filename)
        {
            if (File.Exists(filename))
            {
                return true;
            }
            else
            {
                try
                {
                    File.Create(filename).Close();
                    return true;
                }
                catch(Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// WritePrintersFileAsync
        /// </summary>
        /// <param name="path"></param>
        /// <param name="printers"></param>
        /// <returns></returns>
        public static async Task WritePrintersFileAsync(string path, List<string> printers)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                foreach (string printer in printers)
                {
                    await sw.WriteLineAsync(printer);   
                }
                await sw.FlushAsync();
            }
        }

        /// <summary>
        /// WriteCsvFileAsync
        /// 
        /// Writes a csv file from printer attributes - calls CreateCsvFileEntryAsync.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="printerAttributes"></param>
        /// <returns></returns>
        public static async Task WriteCsvFileAsync(string path, ObservableCollection<IppPrinter> printerAttributes)
        {
            //Write AttributeItems to the file
            using (StreamWriter sw = File.CreateText(path))
            {
                await sw.WriteAsync("IPP Available, Printer Name, Ipp Status, Ipp Version, Mopria Certification, Manufacturer, Firmware, Location, Color\n");
                foreach (IppPrinter ai in printerAttributes) 
                {
                    await Task.Run(() => CreateCsvFileEntryAsync(sw, ai));
                    await sw.FlushAsync();
                }
            }
        }


        /// <summary>
        /// CreateCsvFileEntryAsync
        /// 
        /// Creates printer attributes entries for the csv file
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="ai"></param>
        /// <returns></returns>
        private static async Task CreateCsvFileEntryAsync(StreamWriter sw, IppPrinter ai)
        {
            if (ai.IppUsability == true)
            {
                await sw.WriteAsync("true");
            }
            else
            {
                await sw.WriteAsync("false");
            }
            await sw.WriteAsync("," + ai.Name);
            await sw.WriteAsync("," + ai.Status.ToString());
            await sw.WriteAsync("," + ai.IppVers);
            await sw.WriteAsync("," + ai.Mopria);
            await sw.WriteAsync("," + ai.Manufacturer);
            await sw.WriteAsync("," + ai.Firmware);
            await sw.WriteAsync("," + ai.Location);
            if (ai.Color == true)
            {
                await sw.WriteAsync(",true");
            }
            else
            {
                await sw.WriteAsync(",false");
            }
            // Add the new line...
            await sw.WriteAsync("\n");
        }
        
    }
}
