using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS_GPA_CH3_WPF
{
    /// <summary>
    /// Singleton class for IPP marker properties.
    /// </summary>
    public sealed class MarkerProperties
    {
        private static readonly MarkerProperties instance = new MarkerProperties(); 

        public List<uint> lstMarkerLowLevels = new List<uint>();
        public List<uint> lstMarkerHighLevels = new List<uint>();
        public List<uint> lstMarkerCurrentLevels = new List<uint>();
        public List<string> lstMarkerNames = new List<string>();


        static MarkerProperties()
        {
        }

        private MarkerProperties()
        {

        }

        public static MarkerProperties GetInstance
        {
            get
            {
                return instance;
            }
        }

        public void Clear()
        {
            this.lstMarkerCurrentLevels.Clear();
            this.lstMarkerNames.Clear();
            this.lstMarkerLowLevels.Clear();
            this.lstMarkerHighLevels.Clear();
        }

        public void FillMarkerCollections(string sMarkerCollection, List<string> sMarkerCollectionValues)
        {
          
            if (sMarkerCollection != null) 
            { 
                if(string.Compare(sMarkerCollection, "marker-low-levels", true) == 0)
                {
                    string[] mll = sMarkerCollectionValues.ToArray();
                    foreach (string ll in mll)
                    {
                        this.lstMarkerLowLevels.Add(Convert.ToUInt32(ll));
                    }
                }
                else if (string.Compare(sMarkerCollection, "marker-high-levels", true) == 0)
                {
                    string[] mhl = sMarkerCollectionValues.ToArray();
                    foreach (string hl in mhl)
                    {
                        this.lstMarkerHighLevels.Add(Convert.ToUInt32(hl));
                    }
                }
                else if (string.Compare(sMarkerCollection, "marker-levels", true) == 0)
                {
                    string[] mlc = sMarkerCollectionValues.ToArray();
                    foreach (string mc in mlc)
                    {
                        this.lstMarkerCurrentLevels.Add(Convert.ToUInt32(mc));
                    }
                }
                else if (string.Compare(sMarkerCollection, "marker-names", true) == 0)
                    {
                    string[] mns = sMarkerCollectionValues.ToArray();
                    foreach (string mn in mns)
                    {
                        this.lstMarkerNames.Add(mn);
                    }
                }
            }
            
        }
    }
}


