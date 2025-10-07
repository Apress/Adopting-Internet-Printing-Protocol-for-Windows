using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collections;
using System.Data;

namespace RmLocalPrinters
{
    internal class Program
    {
        private static RemotePrinters m_rps;
        private static List<RemotePrinter> m_nirps = new List<RemotePrinter>();
        private static CommandLineArgs _clas;
        private static Logger _logger;
        static void Main(string[] args)
        {
            try
            {
                _clas = new CommandLineArgs(args);
                _logger = new Logger(_clas.Log);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid command line, reason: {ex.Message}");
                ShowUsage();
                return;
            }

            m_rps = new RemotePrinters();
            try
            {
                QueryRemotePrinters(_clas);
            }
            catch (Exception ex)
            {
                LogAndOutput(ex.Message);
                return;
            }
            m_nirps = m_rps.GetNonIppLocalPrinters();
            if(m_nirps.Count == 0)
            {
                LogAndOutput($"There are no local, non-IPP based printers on {_clas.ComputerName}");
            }
            else
            {
                LogAndOutput($"There are {m_nirps.Count} local, non-IPP based printers on {_clas.ComputerName}");
                {
                    if(_clas.Remove == true)
                    {
                        RemovePrinter(_clas, m_rps);
                    }
                }
            }

            Console.WriteLine("=============================");
        }

       

        static void QueryRemotePrinters(CommandLineArgs clArgs)
        {

            // The security context will be that of the caller - ensure you "Run As" to 
            // run as another context if necessary
            ConnectionOptions options = new ConnectionOptions
            {
                Impersonation = ImpersonationLevel.Impersonate,
                EnablePrivileges = true
            };

            // Create a management scope for the remote computer
            ManagementScope scope = new ManagementScope($"\\\\{clArgs.ComputerName}\\root\\cimv2", options);

            try
            {
                scope.Connect();
                // Query for installed printers
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Printer");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                searcher.Options.Timeout = new TimeSpan(0, 0, (int)clArgs.NumSecs);

                // Execute the query and enumerate the printers
                foreach (ManagementObject printer in searcher.Get())
                {
                    try
                    {
                        RemotePrinter remp = new RemotePrinter(printer);
                        m_rps.AddRemotePrinter(remp);
                    }
                    catch
                    {
                        continue;
                    }
                }
                LogAndOutput($"The following printer objects were found on {clArgs.ComputerName}");
                LogAndOutput(new string('=', 40));
                DisplayRemotePrinters();
                LogAndOutput(new string('=', 40));
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when querying printer on {clArgs.ComputerName}: {ex.Message}");
            }
        }

        /// <summary>
        /// DisplayRemotePrinters
        /// </summary>
        private static void DisplayRemotePrinters()
        {
            foreach (RemotePrinter _rp in m_rps)
            {
                LogAndOutput($"Printer Name: {_rp.Name}");
                LogAndOutput($"Printer Driver: {_rp.DriverName}");
                LogAndOutput($"Printer Port: {_rp.PortName}");
                LogAndOutput($"Printer Status: {_rp.Status}");
                LogAndOutput($"Printer Location: {_rp.Location}");
                if (_rp.Local == true)
                    LogAndOutput("Printer is Local");
                else
                    LogAndOutput("Printer is not local");
                LogAndOutput(new string('-', 40));
            }
        }

        private static void LogAndOutput(string msg)
        {
            Console.WriteLine(msg);
            _logger.Log(msg);   
        }

        private static void RemovePrinter(CommandLineArgs clArgs, RemotePrinters m_rps)
        {

            // The security context will be that of the caller - ensure you "Run As" to 
            // run as another context if necessary
            ConnectionOptions options = new ConnectionOptions
            {
                Impersonation = ImpersonationLevel.Impersonate,
                EnablePrivileges = true
            };


            // Create a management scope for the remote computer
            ManagementScope scope = new ManagementScope($"\\\\{clArgs.ComputerName}\\root\\cimv2", options);

            try
            {
                scope.Connect();
                // Query for installed printers
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Printer");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                searcher.Options.Timeout = new TimeSpan(0, 0, (int)clArgs.NumSecs);

                //LogAndOutput($"The following printer objects were found on {clArgs.ComputerName}");
                LogAndOutput(new string('=', 40));

                // Execute the query and enumerate the printers
                foreach (ManagementObject printer in searcher.Get())
                {
                    try
                    {
                        var localNonIppPrinter = m_nirps.FirstOrDefault(item => item.Name == printer["Name"].ToString());
                        if (localNonIppPrinter != null)
                        {
                            LogAndOutput($"Printer '{localNonIppPrinter.Name}' has been identified as an non-IPP local printer that should be removed...");
                            if (localNonIppPrinter != null)
                            {
                                if (AskToProceed() == true)
                                {
                                    printer.Delete();
                                    LogAndOutput($"Printer '{localNonIppPrinter.Name}' has been deleted successfully.");
                                }
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                LogAndOutput($"An error occurred: {ex.Message}");
            }
        }


        /// <summary>
        /// AskToProceed
        /// </summary>
        /// <returns></returns>
        private static bool AskToProceed()
        {
            string response;

            while (true)
            {
                LogAndOutput("Do you want to proceed? (yes/no)");

                response = Console.ReadLine().Trim().ToLower();

                if (response == "yes")
                {
                    LogAndOutput("You chose to proceed!");
                    return true;
                }
                else if (response == "no")
                {
                    LogAndOutput("You chose not to proceed.");
                    return false;
                }
                else
                {
                    LogAndOutput("Invalid response. Please answer with 'yes' or 'no'.");
                }
            }
        }

        public static void ShowUsage()
        {
            Console.WriteLine("=========== rmLocalPrinters ================");
            Console.WriteLine("Remote local, non-IPP printer remover");
            Console.WriteLine("Usage: rmLocalPrinters /c=<target_computer>");
            Console.WriteLine("Optional switches: /r /l /u=<time in secs");
            Console.WriteLine("Where /r specifies to remove non-IPP local printers found.");
            Console.WriteLine("Where /l specifies logging actions to the file rmLocalPrinters.log in Documents folder");
            Console.WriteLine("Where /u=<timeout in seconds (20 is default)");
            Console.WriteLine("If /r specified, utility will prompt for removal of each non-IPP printer found");
            Console.WriteLine("===========================================");
        }

    }
}
