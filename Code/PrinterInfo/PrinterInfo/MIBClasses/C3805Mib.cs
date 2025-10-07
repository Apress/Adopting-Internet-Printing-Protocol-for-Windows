using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace PrinterInfo
{

    /// <summary>
    /// C3805Mib
    /// 
    /// Approximation of the RFC 3805 Printer MIB2.
    /// </summary>
    public class C3805Mib : CSnmp
    {
        private Hashtable ht_General = new Hashtable();
        private Hashtable ht_Alert = new Hashtable();
        private Hashtable ht_PrintCover = new Hashtable();
        private Hashtable ht_InputStatus = new Hashtable();
        private Hashtable ht_PrtChannelStatus = new Hashtable();
        private Hashtable ht_PrtInterpreterLanguage = new Hashtable();
        private Hashtable ht_PrtConsoleDisplay = new Hashtable();
        private Hashtable ht_PrtMarkerSupplies = new Hashtable();
        private Hashtable ht_PrtMarkerColorant = new Hashtable();
        private Hashtable ht_PrtMediaPathType = new Hashtable();
        private Hashtable ht_PrtPageInfo = new Hashtable();

        const string PRINTER_MIB = "1.3.6.1.2.1.43"; //rfc 3805

        public C3805Mib(int iMaxValues, string cs): base(PRINTER_MIB, iMaxValues, cs)
        {
    
        }

        public void PrintValues()
        {
            foreach (DictionaryEntry de in ht_General)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
            foreach (DictionaryEntry de in ht_Alert)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
            foreach (DictionaryEntry de in ht_PrintCover)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
            foreach (DictionaryEntry de in ht_InputStatus)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
            foreach (DictionaryEntry de in ht_PrtChannelStatus)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
            foreach (DictionaryEntry de in ht_PrtInterpreterLanguage)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
            foreach (DictionaryEntry de in ht_PrtConsoleDisplay)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
            foreach (DictionaryEntry de in ht_PrtMarkerSupplies)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
            foreach (DictionaryEntry de in ht_PrtMarkerColorant)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
            foreach (DictionaryEntry de in ht_PrtMediaPathType)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
            foreach (DictionaryEntry de in ht_PrtPageInfo)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
        }

        public void GetValues()
        {
            GetGeneralSettings();
            iMax = GetMaxValueOfGroup(BASE_OID + ".18.1.1.2.1.1");
            GetAlertSettings(iMax);
            iMax = GetMaxValueOfGroup(BASE_OID + ".6.1.1.2.1.1");
            GetCoverSettings(iMax);
            iMax = GetMaxValueOfGroup(BASE_OID + ".8.2.1.2.1.1");
            GetInputStatus(iMax);
            iMax = GetMaxValueOfGroup(BASE_OID + ".14.1.1.6.1.1");
            GetPrtChannelStatus(iMax);
            iMax = GetMaxValueOfGroup(BASE_OID + ".15.1.1.2.1.1");
            GetInterpreterLanguage(iMax);
            iMax = GetMaxValueOfGroup(BASE_OID + ".16.5.1.2.1.1");
            GetPrtConsoleDisplay(iMax);
            iMax = GetMaxValueOfGroup(BASE_OID + ".11.1.1.5.1.1");
            GetPrtMarkerSupplies(iMax);
            iMax = GetMaxValueOfGroup(BASE_OID + ".12.1.1.4.1.1");
            GetPrtMarkerColorant(iMax);
            iMax = GetMaxValueOfGroup(BASE_OID + ".13.4.1.9.1.1");
            GetPrtMediaPathType(iMax);
            iMax = GetMaxValueOfGroup(BASE_OID + ".10.2.1.4.1.1");
            GetPageCountInfo(iMax);
        }

        /// <summary>
        /// GetGeneralSettings
        /// </summary>
        /// <param name="_cso"></param>
        private void GetGeneralSettings()
        {
            ht_General.Add("max", "1");
            var x = Find(BASE_OID + ".5.1.1.16.1");
            if (x != null)
                ht_General.Add("prtGeneralPrinterName", x.VALUE);
            x = Find(BASE_OID + ".5.1.1.17.1");
            if (x != null)
                ht_General.Add("prtGeneralSerialNumber", x.VALUE);
            x = Find(BASE_OID + ".5.1.1.4.1");
            if (x != null)
                ht_General.Add("prtGeneralCurrentOperator", x.VALUE);
            x = Find(BASE_OID + ".5.1.1.5.1");
            if (x != null)
                ht_General.Add("prtGeneralServicePerson", x.VALUE);
            x = Find(BASE_OID + ".5.1.1.18.1");
            if (x != null)
                ht_General.Add("prtAlertsCriticalEvents", x.VALUE);
            x = Find(BASE_OID + ".5.1.1.19.1");
            if (x != null)
                ht_General.Add("prtAlertsAllEvents", x.VALUE);
            
        }

        /// <summary>
        /// GetAlertSettings
        /// </summary>
        /// <param name="_cso"></param>
        private void GetAlertSettings(int max_index)
        {
            string new_oid = string.Empty;
            ht_Alert.Add("max", max_index.ToString());
            //severity of alert
            string oid_base = BASE_OID + ".18.1.1.2.1.1";
            EnumGroup(oid_base, max_index, ht_Alert, "prtAlertSeverityLevel.1.");
            //training level
            oid_base = BASE_OID + ".18.1.1.3.1.1";
            EnumGroup(oid_base, max_index, ht_Alert, "prtAlertTrainingLevel.1.");
            //group
            oid_base = BASE_OID + ".18.1.1.4.1.1";
            EnumGroup(oid_base, max_index, ht_Alert, "prtAlertGroup.1.");
            //group index
            oid_base = BASE_OID + ".18.1.1.5.1.1";
            EnumGroup(oid_base, max_index, ht_Alert, "prtAlertGroupIndex.1.");
            //location of alert
            oid_base = BASE_OID + ".18.1.1.6.1.1";
            EnumGroup(oid_base, max_index, ht_Alert, "prtAlertLocation.1.");
            //alerts code
            oid_base = BASE_OID + ".18.1.1.7.1.1";
            EnumGroup(oid_base, max_index, ht_Alert, "prtAlertCode.1.");
            //alerts description
            oid_base = BASE_OID + ".18.1.1.8.1.1";
            EnumGroup(oid_base, max_index, ht_Alert, "prtAlertDescription.1.");
            //time of alert since power on
            oid_base = BASE_OID + ".18.1.1.9.1.1";
            EnumGroup(oid_base, max_index, ht_Alert, "prtAlertTime.1.");
        }

        /// <summary>
        /// GetCoverSettings
        /// </summary>
        /// <param name="_cso"></param>
        private void GetCoverSettings(int max_index)
        {
            string new_oid = string.Empty;
            ht_PrintCover.Add("max", max_index.ToString());
            // cover name
            string oid_base = BASE_OID + ".6.1.1.2.1.1";
            EnumGroup(oid_base, max_index, ht_PrintCover, "prtCoverDescription.1.");
            // cover status
            oid_base = BASE_OID + ".6.1.1.3.1.1";
            EnumGroup(oid_base, max_index, ht_PrintCover, "prtCoverStatus.1.");

        }

        private void GetInputStatus(int max_index)
        {
            string new_oid = string.Empty;
            ht_InputStatus.Add("max", max_index.ToString());
            // input type
            
            // input type
            string oid_base = BASE_OID + ".8.2.1.8.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtInputCapacityUnit.1.");
            // max capacity
            oid_base = BASE_OID + ".8.2.1.9.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtInputMaxCapacity.1.");
            // current capacity
            oid_base = BASE_OID + ".8.2.1.10.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtInputCurrentLevel.1.");
            // input status
            oid_base = BASE_OID + ".8.2.1.11.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtInputStatus.1.");
            // media name
            oid_base = BASE_OID + ".8.2.1.12.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtInputMediaName.1.");
            // input name
            oid_base = BASE_OID + ".8.2.1.13.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtInputName.1.");
            // input serial number
            oid_base = BASE_OID + ".8.2.1.17.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtInputSerialNumber.1.");
            // media type
            oid_base = BASE_OID + ".8.2.1.21.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtInputMediaType.1.");
            // media color
            oid_base = BASE_OID + ".8.2.1.22.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtInputMediaColor.1.");
        }


        //print channel state/status
        private void GetPrtChannelStatus(int max_index)
        {
            string new_oid = string.Empty;
            ht_PrtChannelStatus.Add("max", max_index.ToString());
            // prtChannelState
            string oid_base = BASE_OID + ".14.1.1.6.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtChannelState.1.");
            // prtChannelStatus
            oid_base = BASE_OID + ".14.1.1.8.1.1";
            EnumGroup(oid_base, max_index, ht_InputStatus, "prtChannelStatus.1.");
        }

        //print interpreter language info 
        private void GetInterpreterLanguage(int max_index)
        {
            string new_oid = string.Empty;
            ht_PrtInterpreterLanguage.Add("max", max_index.ToString());
            //Lang familly
            string oid_base = BASE_OID + ".15.1.1.2.1.1";
            EnumGroup(oid_base, max_index, ht_PrtInterpreterLanguage, "prtInterpreterLangFamilly.1.");
            //Lang Level
            oid_base = BASE_OID + ".15.1.1.3.1.1";
            EnumGroup(oid_base, max_index, ht_PrtInterpreterLanguage, "prtInterpreterLangLevel.1.");
            //Lang Version
            oid_base = BASE_OID + ".15.1.1.4.1.1";
            EnumGroup(oid_base, max_index, ht_PrtInterpreterLanguage, "prtInterpreterLangVersion.1.");
            //Lang Description
            oid_base = BASE_OID + ".15.1.1.5.1.1";
            EnumGroup(oid_base, max_index, ht_PrtInterpreterLanguage, "prtInterpreterLangDescription.1.");
            //Interpreter Version
            oid_base = BASE_OID + ".15.1.1.6.1.1";
            EnumGroup(oid_base, max_index, ht_PrtInterpreterLanguage, "prtInterpreterVersion.1.");
            //Interpreter default Orientation
            oid_base = BASE_OID + ".15.1.1.7.1.1";
            EnumGroup(oid_base, max_index, ht_PrtInterpreterLanguage, "prtInterpreterDefaultOrientation.1.");
            //Interpreter Two Way 
            oid_base = BASE_OID + ".15.1.1.12.1.1";
            EnumGroup(oid_base, max_index, ht_PrtInterpreterLanguage, "prtInterpreterTwoWay.1.");
        }

        //print supplies
        private void GetPrtMarkerSupplies(int max_index)
        {
            string new_oid = string.Empty;
            ht_PrtMarkerSupplies.Add("max", max_index.ToString());
            // print marker supplies type
            string oid_base = BASE_OID + ".11.1.1.5.1.1";
            EnumGroup(oid_base, max_index, ht_PrtMarkerSupplies, "prtMarkerSuppliesType.1.");
            //print marker supplies description
            oid_base = BASE_OID + ".11.1.1.6.1.1";
            EnumGroup(oid_base, max_index, ht_PrtMarkerSupplies, "prtMarkerSuppliesDescription.1.");
            //print marker supplies supply unit
            oid_base = BASE_OID + ".11.1.1.7.1.1";
            EnumGroup(oid_base, max_index, ht_PrtMarkerSupplies, "prtMarkerSuppliesSupplyUnit.1.");
            //print marker supplies max capacity
            oid_base = BASE_OID + ".11.1.1.8.1.1";
            EnumGroup(oid_base, max_index, ht_PrtMarkerSupplies, "prtMarkerSuppliesMaxCapacity.1.");
            //print marker supplies level
            oid_base = BASE_OID + ".11.1.1.9.1.1";
            EnumGroup(oid_base, max_index, ht_PrtMarkerSupplies, "prtMarkerSuppliesLevel.1.");
        }

         //print colorant
        private void GetPrtMarkerColorant(int max_index)
        {
            string new_oid = string.Empty;
            ht_PrtMarkerColorant.Add("max", max_index.ToString());
            // print marker colorant value
            string oid_base = BASE_OID + ".12.1.1.4.1.1";
            EnumGroup(oid_base, max_index, ht_PrtMarkerColorant, "prtMarkerColorantValue.1.");
            // print marker tonality
            oid_base = BASE_OID + ".12.1.1.5.1.1";
            EnumGroup(oid_base, max_index, ht_PrtMarkerColorant, "prtMarkerColorantTonality.1.");
        }


        //print console display
        private void GetPrtConsoleDisplay(int max_index)
        {
            string new_oid = string.Empty;
            ht_PrtConsoleDisplay.Add("max", max_index.ToString());
            // console buffer text
            string oid_base = BASE_OID + ".16.5.1.2.1.1";
            EnumGroup(oid_base, max_index, ht_PrtConsoleDisplay, "prtConsoleDisplayBufferText.1.");
        }

         //print media path type (simplex/duplex...)
        private void GetPrtMediaPathType(int max_index)
        {
            string new_oid = string.Empty;
            ht_PrtMediaPathType.Add("max", max_index.ToString());
            // print media path type
            string oid_base = BASE_OID + ".13.4.1.9.1.1";
            EnumGroup(oid_base, max_index, ht_PrtMediaPathType, "prtMediaPathType.1.");
            // print media path description
            oid_base = BASE_OID + ".13.4.1.10.1.1";
            EnumGroup(oid_base, max_index, ht_PrtMediaPathType, "prtMediaPathDescription.1.");
            // print media path status
            oid_base = BASE_OID + ".13.4.1.11.1.1";
            EnumGroup(oid_base, max_index, ht_PrtMediaPathType, "prtMediaPathStatus.1.");
        }

        private void GetPageCountInfo(int max_index)
        {
            //1.3.6.1.2.1.43.10.2.1.4.1.1
            string new_oid = string.Empty;
            ht_PrtPageInfo.Add("max", max_index.ToString());
            // print media path type
            string oid_base = BASE_OID + ".10.2.1.4.1.1";
            EnumGroup(oid_base, max_index, ht_PrtPageInfo, "prtPageCount.1.");
        }

    }
}
