using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddSharedIppPrinters.Helpers
{
    /// <summary>
    /// CLProcessor
    /// 
    /// Singleton class for
    /// command line processing 
    /// </summary>
    internal class CLProcessor
    {
        private string m_sPrintServer;
        private List<string> m_sLocations = new List<string>(); 
        private uint clOptions;

        public const int PURGE = 1;
        public const int PRINT_SERVER = 2;
        public const int PRINT_LOCATIONS = 4;

        private static readonly CLProcessor instance = new CLProcessor();

        static CLProcessor()
        {

        }

        private CLProcessor()
        {

        }

        public static CLProcessor GetInstance
        {
            get
            {
                return instance;
            }
        }

        public string PrintServer 
        { 
            get => m_sPrintServer; 
            set => m_sPrintServer = value; 
        }

        /// <summary>
        /// IsMatchingLocation
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool IsMatchingLocation(string location)
        {
            foreach (string s in m_sLocations)
            {
                if(string.Compare(s, location, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }
       
        /// <summary>
        /// GetOptions
        /// 
        /// Used by caller to see if a command line option binary value
        /// is a chosen option.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public bool GetOptions(uint option)
        {
            if ((clOptions & option) == option)
            {
                return true;    
            }
            else
            {
                return false;   
            }
        }

        /// <summary>
        /// GetCommandLineArgs
        /// 
        /// Retrieve and process the command line arguments
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        public void GetCommandLineArgs(string[] args)
        {
            int i = 0;
            foreach (string arg in args)
            {
                if (string.Compare(arg, "/purge", true) == 0)
                    clOptions |= CLProcessor.PURGE;
                else if (arg.StartsWith("/ps=", true, CultureInfo.CurrentCulture))
                {
                    int indx = arg.IndexOf("=");
                    if (indx == Defines.npos)
                    {
                        throw new Exception("Could not retrieve name of print server");
                    }
                    else
                    {
                        PrintServer = arg.Substring(indx + 1);
                        clOptions |= CLProcessor.PRINT_SERVER;
                    }
                }
                else if (arg.StartsWith("/pl=", true, CultureInfo.CurrentCulture))
                {
                    int indx = arg.IndexOf("=");
                    if (indx == Defines.npos)
                    {
                        throw new Exception("Could not retrieve name of printer locations");
                    }
                    else
                    {
                        try
                        {
                            string sl = arg.Substring(indx + 1);
                            m_sLocations = sl.Split(',').ToList();
                            clOptions |= CLProcessor.PRINT_LOCATIONS;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Invalid print locations list: " + ex.Message);
                        }
                    }
                }
                else
                {
                    throw new Exception("Unknown command line arguments..");
                }
                i++;
            }

            if(GetOptions(PRINT_SERVER) == false)
            {
                throw new Exception("Command line must spcify a print server");
            }
        }
        
    }
}
