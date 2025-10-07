using CsIppRequestLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    /// <summary>
    /// Mappings
    /// 
    /// This static class provides IPP enum to string mappings
    /// </summary>
    public static class Mappings
    {
        public const int ENUM = 0x023;
        public static readonly string[] avnames = { "print-quality-default", "print-quality-supported", "media-type", "printer-state", "orientation-requested", "orientation-requested-default", "orientation-requested-supported", "operations-supported", "finishings-default", "finishings-ready", "finishings-supported" };

        public static bool IsMapped(string name)
        {
            foreach (string avname in avnames)
            {
                if(string.Compare(name, avname, StringComparison.OrdinalIgnoreCase) == 0) 
                    return true;
            }
            return false;
        }

        public static Dictionary<string, Dictionary<int, string>> enumMappings = new Dictionary<string, Dictionary<int, string>>
        {
            { "print-quality-default", new Dictionary<int, string>
                {
                    { 3, "Draft" },
                    { 4, "Normal" },
                    { 5, "High" }
                }
            },
            { "print-quality-supported", new Dictionary<int, string>
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
            { "orientation-requested", new Dictionary<int, string>
                {
                    { 3, "Portrait" },
                    { 4, "Landscape" },
                    { 5, "Reverse-Landscape" },
                    { 6, "Reverse-Portrait" },
                    { 7, "none" }
                }
            },
            { "orientation-requested-default", new Dictionary<int, string>
                {
                    { 3, "Portrait" },
                    { 4, "Landscape" },
                    { 5, "Reverse-Landscape" },
                    { 6, "Reverse-Portrait" },
                    { 7, "none" }
                }
            },
            { "orientation-requested-supported", new Dictionary<int, string>
                {
                    { 3, "Portrait" },
                    { 4, "Landscape" },
                    { 5, "Reverse-Landscape" },
                    { 6, "Reverse-Portrait" },
                    { 7, "none" }
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
                    { 18, "Purge-Jobs" },
                    { 19, "Set-Printer-Attributes" },
                    { 20, "Set-Job-Attributes" },
                    { 21, "Get-Printer-Supported-Values"},
                    { 22, "Create-Printer-Subscriptions"},
                    { 23, "Create-Job-Subscriptions"},
                    { 24, "Get-Subscription-Attributes"},
                    { 25, "Get-Subscriptions"},
                    { 26, "Renew-Subscription"},
                    { 27, "Cancel-Subscription"},
                    { 28, "Get-Notifications"},
                    { 29, "Reserved"},
                    { 30, "Get-Resource-Attributes"},
                    { 31, "Reserved"},
                    { 32, "Get-Resources"},
                    { 33, "Reserved"},
                    { 34, "Enable-Printer"},
                    { 35, "Disable-Printer"},
                    { 36, "Pause-Printer-After-Current-Job"},
                    { 37, "Hold-New-Jobs"},
                    { 38, "Release-Held-New-Jobs"},
                    { 39, "Deactivate-Printer"},
                    { 40, "Activate-Printer"},
                    { 41, "Restart-Printer"},
                    { 42, "Shutdown-Printer"},
                    { 43, "Startup-Printer"},
                    { 44, "Reprocess-Job"},
                    { 45, "Cancel-Current-Job"},
                    { 46, "Suspend-Current-Job"},
                    { 47, "Resume-Job"},
                    { 48, "Promote-Job"},
                    { 49, "Schedule-Job-After"},
                    { 50, "Cancel-Document"},
                    { 51, "Get-Document-Attributes"},
                    { 52, "Get-Documents"},
                    { 53, "Delete-Document"},
                    { 54, "Set-Document-Attributes"},
                    { 55, "Cancel-Jobs"},
                    { 56, "Cancel-My-Jobs"},
                    { 57, "Resubmit-Job"},
                    { 58, "Close-Job"},
                    { 59, "Identify-Printer"},
                    { 60, "Validate-Document"},
                }
            },
            { "finishings-default", new Dictionary<int, string>
                {
                    { 3, "none" },
                    { 4, "staple" },
                    { 5, "punch" },
                    { 6, "cover" },
                    { 7, "bind" },
                    { 8, "saddle-stitch" },
                    { 9, "edge-stitch" },
                    { 10, "fold" },
                    { 11, "trim" },
                    { 12, "bale" },
                    { 13, "booklet-maker" },
                    { 14, "jog-offset" },
                    { 15, "coat" },
                    { 16, "laminate" },
                    { 20, "staple-top-left" },
                    { 21, "staple-bottom-left" },
                    { 22, "staple-top-right" },
                    { 23, "staple-bottom-right" },
                    { 24, "edge-stitch-left" },
                    { 25, "edge-stitch-top" },
                    { 26, "edge-stitch-right" },
                    { 27, "edge-stitch-bottom" },
                    { 28, "staple-dual-left" },
                    { 29, "staple-dual-top" },
                    { 30, "staple-dual-right" },
                    { 31, "staple-dual-bottom" },
                    { 32, "staple-triple-left" },
                    { 33, "staple-triple-top" },
                    { 34, "staple-triple-right" },
                    { 35, "staple-triple-bottom" },
                    { 50, "bind-left" },
                    { 51, "bind-top" },
                    { 52, "bind-right" },
                    { 53, "bind-bottom" },
                    { 60, "trim-after-pages" },
                    { 61, "trim-after-documents" },
                    { 62, "trim-after-copies" },
                    { 63, "trim-after-job" },
                    { 70, "punch-top-left" },
                    { 71, "punch-bottom-left" },
                    { 72, "punch-top-right" },
                    { 73, "punch-bottom-right" },
                    { 74, "punch-dual-left" },
                    { 75, "punch-dual-top" },
                    { 76, "punch-dual-right" },
                    { 77, "punch-dual-bottom" },
                    { 78, "punch-triple-left" },
                    { 79, "punch-triple-top" },
                    { 80, "punch-triple-right" },
                    { 81, "punch-triple-bottom" },
                    { 82, "punch-quad-left" },
                    { 83, "punch-quad-top" },
                    { 84, "punch-quad-right" },
                    { 85, "punch-quad-bottom" },
                    { 86, "punch-multiple-left" },
                    { 87, "punch-multiple-top" },
                    { 88, "punch-multiple-right" },
                    { 89, "punch-multiple-bottom" },
                    { 90, "fold-accordion" },
                    { 91, "fold-double-gate" },
                    { 92, "fold-gate" },
                    { 93, "fold-half" },
                    { 94, "fold-half-z" },
                    { 95, "fold-left-gate" },
                    { 96, "fold-letter" },
                    { 97, "fold-parallel" },
                    { 98, "fold-poster" },
                    { 99, "fold-right-gate" },
                    { 100, "fold-z" },
                    { 101, "fold-engineering-z" }
                }
            },
             { "finishings-ready", new Dictionary<int, string>
                {
                    { 3, "none" },
                    { 4, "staple" },
                    { 5, "punch" },
                    { 6, "cover" },
                    { 7, "bind" },
                    { 8, "saddle-stitch" },
                    { 9, "edge-stitch" },
                    { 10, "fold" },
                    { 11, "trim" },
                    { 12, "bale" },
                    { 13, "booklet-maker" },
                    { 14, "jog-offset" },
                    { 15, "coat" },
                    { 16, "laminate" },
                    { 20, "staple-top-left" },
                    { 21, "staple-bottom-left" },
                    { 22, "staple-top-right" },
                    { 23, "staple-bottom-right" },
                    { 24, "edge-stitch-left" },
                    { 25, "edge-stitch-top" },
                    { 26, "edge-stitch-right" },
                    { 27, "edge-stitch-bottom" },
                    { 28, "staple-dual-left" },
                    { 29, "staple-dual-top" },
                    { 30, "staple-dual-right" },
                    { 31, "staple-dual-bottom" },
                    { 32, "staple-triple-left" },
                    { 33, "staple-triple-top" },
                    { 34, "staple-triple-right" },
                    { 35, "staple-triple-bottom" },
                    { 50, "bind-left" },
                    { 51, "bind-top" },
                    { 52, "bind-right" },
                    { 53, "bind-bottom" },
                    { 60, "trim-after-pages" },
                    { 61, "trim-after-documents" },
                    { 62, "trim-after-copies" },
                    { 63, "trim-after-job" },
                    { 70, "punch-top-left" },
                    { 71, "punch-bottom-left" },
                    { 72, "punch-top-right" },
                    { 73, "punch-bottom-right" },
                    { 74, "punch-dual-left" },
                    { 75, "punch-dual-top" },
                    { 76, "punch-dual-right" },
                    { 77, "punch-dual-bottom" },
                    { 78, "punch-triple-left" },
                    { 79, "punch-triple-top" },
                    { 80, "punch-triple-right" },
                    { 81, "punch-triple-bottom" },
                    { 82, "punch-quad-left" },
                    { 83, "punch-quad-top" },
                    { 84, "punch-quad-right" },
                    { 85, "punch-quad-bottom" },
                    { 86, "punch-multiple-left" },
                    { 87, "punch-multiple-top" },
                    { 88, "punch-multiple-right" },
                    { 89, "punch-multiple-bottom" },
                    { 90, "fold-accordion" },
                    { 91, "fold-double-gate" },
                    { 92, "fold-gate" },
                    { 93, "fold-half" },
                    { 94, "fold-half-z" },
                    { 95, "fold-left-gate" },
                    { 96, "fold-letter" },
                    { 97, "fold-parallel" },
                    { 98, "fold-poster" },
                    { 99, "fold-right-gate" },
                    { 100, "fold-z" },
                    { 101, "fold-engineering-z" }
                }
            },
              { "finishings-supported", new Dictionary<int, string>
                {
                    { 3, "none" },
                    { 4, "staple" },
                    { 5, "punch" },
                    { 6, "cover" },
                    { 7, "bind" },
                    { 8, "saddle-stitch" },
                    { 9, "edge-stitch" },
                    { 10, "fold" },
                    { 11, "trim" },
                    { 12, "bale" },
                    { 13, "booklet-maker" },
                    { 14, "jog-offset" },
                    { 15, "coat" },
                    { 16, "laminate" },
                    { 20, "staple-top-left" },
                    { 21, "staple-bottom-left" },
                    { 22, "staple-top-right" },
                    { 23, "staple-bottom-right" },
                    { 24, "edge-stitch-left" },
                    { 25, "edge-stitch-top" },
                    { 26, "edge-stitch-right" },
                    { 27, "edge-stitch-bottom" },
                    { 28, "staple-dual-left" },
                    { 29, "staple-dual-top" },
                    { 30, "staple-dual-right" },
                    { 31, "staple-dual-bottom" },
                    { 32, "staple-triple-left" },
                    { 33, "staple-triple-top" },
                    { 34, "staple-triple-right" },
                    { 35, "staple-triple-bottom" },
                    { 50, "bind-left" },
                    { 51, "bind-top" },
                    { 52, "bind-right" },
                    { 53, "bind-bottom" },
                    { 60, "trim-after-pages" },
                    { 61, "trim-after-documents" },
                    { 62, "trim-after-copies" },
                    { 63, "trim-after-job" },
                    { 70, "punch-top-left" },
                    { 71, "punch-bottom-left" },
                    { 72, "punch-top-right" },
                    { 73, "punch-bottom-right" },
                    { 74, "punch-dual-left" },
                    { 75, "punch-dual-top" },
                    { 76, "punch-dual-right" },
                    { 77, "punch-dual-bottom" },
                    { 78, "punch-triple-left" },
                    { 79, "punch-triple-top" },
                    { 80, "punch-triple-right" },
                    { 81, "punch-triple-bottom" },
                    { 82, "punch-quad-left" },
                    { 83, "punch-quad-top" },
                    { 84, "punch-quad-right" },
                    { 85, "punch-quad-bottom" },
                    { 86, "punch-multiple-left" },
                    { 87, "punch-multiple-top" },
                    { 88, "punch-multiple-right" },
                    { 89, "punch-multiple-bottom" },
                    { 90, "fold-accordion" },
                    { 91, "fold-double-gate" },
                    { 92, "fold-gate" },
                    { 93, "fold-half" },
                    { 94, "fold-half-z" },
                    { 95, "fold-left-gate" },
                    { 96, "fold-letter" },
                    { 97, "fold-parallel" },
                    { 98, "fold-poster" },
                    { 99, "fold-right-gate" },
                    { 100, "fold-z" },
                    { 101, "fold-engineering-z" }
                }
            }
        };
    }
}
/*---------------Usage---------------------
private void EnumToValue(IppAttributeCollection _pac)
{
    foreach (string sAttrName in Mappings.avnames)
    {
        IppAttribute pa = _pac.Find(sAttrName);
        if (pa != null)
        {
            int i = 0;
            foreach (string enumString in pa.AttributeValues)
            {
                if (Mappings.enumMappings.TryGetValue(pa.Name, out var attributeMappings))
                {
                    if (attributeMappings.TryGetValue(Convert.ToInt32(enumString), out var enumStringValue))
                    {
                        pa.AttributeValues[i] += " ";
                        pa.AttributeValues[i] += enumStringValue;
                    }
                }
                i++;
            }
        }
    }
}
------------------------------------------------------------*/