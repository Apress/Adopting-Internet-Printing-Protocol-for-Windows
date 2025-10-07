using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Printers;
using Windows.Devices.Printers.Extensions;
using Windows.Storage.Streams;
using System.Printing;
using System.Collections;
using Microsoft.Win32;


namespace CheckWppEnv
{
    internal class Program
    {
        static string sComputer;
        static bool bEnable = false;
        static bool bDisable = false;
        static bool bResults = false;
        static void Main(string[] args)
        {
            try
            {
                CheckCommandLine(args);
                if((bEnable == true) && (bDisable == true))
                {
                    throw new Exception("Invalid command line: enable and disable WPP both chosen..");
                }
                if (string.IsNullOrEmpty(sComputer))
                {
                    throw new Exception("Invalid command line: a computer must be specified..");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                ShowUsage();
                return;
            }

            try
            {
                GetIppPrintersOnMachine(sComputer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            try
            {
                bResults = IsWppEnabled(sComputer);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if((bEnable == true) && (bResults == false))
            {
                TurnWppOn(sComputer);
            }

            if ((bDisable == true) && (bResults == true))
            {
                TurnWppOff(sComputer);
            }
        }

        /// <summary>
        /// IsWppEnabled
        /// 
        /// Determine if WPP is enabled on remote machine
        /// </summary>
        /// <param name="computer"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool IsWppEnabled(string computer)
        {
            const string keyPath = @"SOFTWARE\Policies\Microsoft\Windows NT\Printers\WPP";
            const string valueName = "EnabledBy";

            try
            {
                // Open the remote registry base key (LocalMachine hive)
                using (RegistryKey remoteBaseKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, computer))
                {
                    // Open the subkey for Protected Print
                    using (RegistryKey protectedPrintKey = remoteBaseKey.OpenSubKey(keyPath))
                    {
                        if (protectedPrintKey != null)
                        {
                            object value = protectedPrintKey.GetValue(valueName);
                            if (value != null && value is int intValue && intValue == 2)
                            {
                                Console.WriteLine($"Windows Protected Print is ENABLED on {computer}.");
                                return true;
                            }
                            else
                            {
                                Console.WriteLine($"Windows Protected Print is NOT enabled on {computer}.");
                                return false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Protected Print registry key not found on {computer}.");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error accessing registry on {computer}: {ex.Message}");
            }
        }

        /// <summary>
        /// TurnWppOff
        /// 
        /// Disable WPP on remote machine
        /// </summary>
        /// <param name="computer"></param>
        public static void TurnWppOff(string computer)
        {
            try
            {
                using (RegistryKey remoteBaseKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, computer))
                {
                    // Delete the subkey and all its subkeys/values
                    remoteBaseKey.DeleteSubKeyTree(@"SOFTWARE\Policies\Microsoft\Windows NT\Printers\WPP", throwOnMissingSubKey: false);

                    Console.WriteLine("WPP Disabled on the remote machine.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied. Ensure you have administrative privileges on the remote machine.");
            }
            catch (System.IO.IOException ioEx)
            {
                Console.WriteLine("I/O error: " + ioEx.Message);
                Console.WriteLine("Check if the remote machine is reachable and the Remote Registry service is running.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// TurnWppOn
        /// 
        /// Enable WPP on remote machine
        /// </summary>
        /// <param name="computer"></param>
        public static void TurnWppOn(string computer)
        {
            try
            {
                using (RegistryKey remoteBaseKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, computer))
                {
                    // Open or create the subkey with write access
                    using (RegistryKey key = remoteBaseKey.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Printers\WPP"))
                    {
                        if (key == null)
                        {
                            Console.WriteLine("Failed to open or create the registry key on the remote machine.");
                            return;
                        }

                        key.SetValue("EnabledBy", 2, RegistryValueKind.DWord);
                        key.SetValue("WindowsProtectedPrintGroupPolicyState", 1, RegistryValueKind.DWord);
                        key.SetValue("WindowsProtectedPrintMode", 1, RegistryValueKind.DWord);
                        key.SetValue("WindowsProtectedPrintOobeConfigComplete", 1, RegistryValueKind.DWord);

                        Console.WriteLine("WPP Enabled on the remote machine.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied. Ensure you have administrative privileges on the remote machine.");
            }
            catch (System.IO.IOException ioEx)
            {
                Console.WriteLine("I/O error: " + ioEx.Message);
                Console.WriteLine("Check if the remote machine is reachable and the Remote Registry service is running.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

        }

        /// <summary>
        /// GetIppPrintersOnMachine
        /// 
        /// View IPP printers on remote machine
        /// </summary>
        /// <param name="computer"></param>
        public static void GetIppPrintersOnMachine(string computer)
        {
            try
            {
                // Connect to the remote print server
                PrintServer printServer = new PrintServer(@"\\" + computer);

                // Get the collection of print queues (printers)
                PrintQueueCollection printQueues = printServer.GetPrintQueues();

                Console.WriteLine($"Printers using IPP class driver on {computer}:");

                foreach (PrintQueue pq in printQueues)
                {
                    // Refresh the properties to get the latest info
                    pq.Refresh();

                    // The DriverName property contains the driver name
                    string driverName = pq.QueueDriver.Name;

                    // Check if the driver name contains "IPP" (case-insensitive)
                    if (!string.IsNullOrEmpty(driverName) && driverName.IndexOf("IPP", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Console.WriteLine($"Name: {pq.Name}, Driver: {driverName})");
                        Console.WriteLine($"Port: {pq.QueuePort.Name}, Status: {pq.QueueStatus})");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }


        /// <summary>
        /// CheckCommandLine
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        private static void CheckCommandLine(string[] args)
        {

            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    string sSwitch = args[i].ToString().Substring(0, 2).Trim();
                    if (sSwitch == "/c")
                    {
                        sComputer = args[i].ToString().Substring(3).Trim();
                    }
                    else if (sSwitch == "/e")
                    {
                        bEnable = true;
                    }
                    else if (sSwitch == "/d")
                    {
                        bDisable = true;
                    }
                    else
                    {
                        throw new Exception("Invalid command line switch provided!");
                    }
                }
                catch (Exception)
                {
                    throw new Exception("Error processing command line!");
                }
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage: CheckWppEnv /c=<computer> [/e] [/d]");
            Console.WriteLine("  /c=<computer>  Specify the target computer name (required).");
            Console.WriteLine("Options:");
            Console.WriteLine("  /e  Enable Windows Protected Print on the target computer.");
            Console.WriteLine("  /d  Disable Windows Protected Print on the target computer.");
            Console.WriteLine("Note: You must run this application with administrative privileges.");
        }
         
    }
}

