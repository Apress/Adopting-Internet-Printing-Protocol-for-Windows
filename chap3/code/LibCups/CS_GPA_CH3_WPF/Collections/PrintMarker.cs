using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_GPA_CH3_WPF
{/// <summary>
/// PrintMarker
/// </summary>
    public class PrintMarker
    {
        private uint level;
        private uint highlevel;
        private uint lowlevel;
        private string sLevel;
        private string sHighLevel;
        private string sLowLevel;
        private string name;

        public PrintMarker(string n, uint l, uint hl, uint ll)
        {
            Name = n;
            if (l >= 0 && l <= 100)
            {
                Level = l;
                StringLevel = l.ToString();
            }
            else
            {
                level = 0;
                StringLevel = "0";
            }
            if (ll >= 0 && ll <= 100)
            {
                Lowlevel = ll;
                StringLowLevel = ll.ToString();
            }
            else
            {
                Lowlevel = 0;
                StringLowLevel = "0";
            }
            if (hl >= 0 && hl <= 100)
            {
                Highlevel = hl;
                StringHighLevel = hl.ToString();
            }
            else
            {
                Highlevel = 0;
                StringHighLevel = "0";
            }
        }

        public uint Level
        { 
            get => level; 
            set => level = value; 
        }
        public string StringLevel
        { 
            get => sLevel; 
            set => sLevel = value; 
        }
        public string Name 
        { 
            get => name; 
            set => name = value; 
        }
        public uint Highlevel 
        { 
            get => highlevel; 
            set => highlevel = value; 
        }
        public uint Lowlevel 
        { 
            get => lowlevel; 
            set => lowlevel = value; 
        }
        public string StringHighLevel 
        { 
            get => sHighLevel; 
            set => sHighLevel = value; 
        }
        public string StringLowLevel 
        { 
            get => sLowLevel; 
            set => sLowLevel = value; 
        }
    }
}
