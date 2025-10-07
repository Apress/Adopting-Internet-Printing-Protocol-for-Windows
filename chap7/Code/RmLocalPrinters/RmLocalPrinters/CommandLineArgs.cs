using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmLocalPrinters
{

   
    public class CommandLineArgs
    {
        private const int npos = -1;
        private string m_sComputerName = string.Empty;
        private bool m_bRemove;
        private bool m_bDisplay;
        private uint m_uiNumSecs;
        private bool m_bLog;

        public CommandLineArgs(string[] args)
        {
            m_uiNumSecs = 20;
            m_bDisplay = true;
            m_bRemove = false;
            m_bLog = false; 
            ProcessCommandLine(args);
        }

        private void ProcessCommandLine(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.ToLower().StartsWith("/c="))
                {
                    int pos = arg.IndexOf('=');
                    if(pos == npos)
                    {
                        throw new ArgumentException("Invalid computer name recovered in command line (/c=<computer>)");
                    }
                    else
                    {
                        ComputerName = arg.Substring(pos + 1);   
                    }
                }
                else if (arg.ToLower().StartsWith("/r"))
                {
                    Display = false;
                    Remove = true;
                }
                else if (arg.ToLower().StartsWith("/l"))
                {
                    Log = true;
                }
                else if (arg.ToLower().StartsWith("/t="))
                {
                    int pos = arg.IndexOf('=');
                    if (pos == npos)
                    {
                        throw new ArgumentException("Invalid timeout in command line (/t=<num seconds>)");
                    }
                    else
                    {
                        try
                        {
                            Convert.ToUInt64 (arg.Substring(pos + 1));  
                        }
                        catch (Exception e)
                        {
                            throw new ArgumentException("Invalid timeout value in command line (/t=<num seconds>)");
                        }
                    }
                }
                else
                {
                    throw new ArgumentException($"Invalid command line argument found: {arg}");
                }
            }
            if (ComputerName.Length == 0)
            {
                throw new ArgumentException("Invalid command line, computer name not found!");
            }

        }

       

        public string ComputerName { get => m_sComputerName; set => m_sComputerName = value; }
        public bool Remove { get => m_bRemove; set => m_bRemove = value; }
        public bool Display { get => m_bDisplay; set => m_bDisplay = value; }
        public uint NumSecs { get => m_uiNumSecs; set => m_uiNumSecs = value; }
        public bool Log { get => m_bLog; set => m_bLog = value; }
    }
}
