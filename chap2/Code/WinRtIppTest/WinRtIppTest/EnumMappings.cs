using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinRtIppTest
{
    public static class Mappings
    {
        public static Dictionary<string, Dictionary<int, string>> enumMappings = new Dictionary<string, Dictionary<int, string>>
        {
            { "print-quality", new Dictionary<int, string>
                {
                    { 3, "Draft" },
                    { 4, "Normal" },
                    { 5, "High" }
                }
            },
            { "media-type", new Dictionary<int, string>
                {
                    { 0, "Auto" },
                    { 1, "Plain" },
                    { 2, "Photo" }
                }
            },
              { "printer-state", new Dictionary<int, string>
                {
                    { 3, "Idle" },
                    { 4, "Processing" },
                    { 5, "Stopped" }
                }
            },
            //RFC 8011, 5.4.15. operations-supported
            { "operations-supported", new Dictionary<int, string>
                {
                    { 0, "Reserved" },
                    { 1, "Reserved" },
                    { 2, "Print-Job" },
                    { 3, "Print-URI" },
                    { 4, "Validate-Job" },
                    { 5, "Create-Job" },
                    { 6, "Send-Document" },
                    { 7, "Send-URI" },
                    { 8, "Cancel-Job" },
                    { 9, "Get-Job_Attributes" },
                    { 10, "Get-Jobs" },
                    { 11, "Get-Printer-Attributes" },
                    { 12, "Hold-Jobs" },
                    { 13, "Release-Job" },
                    { 14, "Restart-Job" },
                    { 15, "Reserved" },
                    { 16, "Pause-Printer" },
                    { 17, "Resume-Printer" },
                    { 18, "Purge-Jobs" }
                }
            }
        };

    }
}
