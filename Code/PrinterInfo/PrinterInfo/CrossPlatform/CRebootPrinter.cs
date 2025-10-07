using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnmpSharpNet;
using System.Net;

namespace PrinterInfo
{
    public static class CRebootPrinter
    {
       static string sIpAddress = string.Empty;
       static IPAddress ipaTarget = null;
       static string sCommunityString = "public";
       static bool bIPAddress = true;
       static bool bReturn = false;
       static string sReturnMessage = string.Empty;
       private const string REBOOT_OID = "1.3.6.1.2.1.43.5.1.1.3.1.4";

       public static string RETURN_MESSAGE
       {
           get { return CRebootPrinter.sReturnMessage; }
           set { CRebootPrinter.sReturnMessage = value; }
       }

       public static string COMMUNITY_STRING
       {
           get { return sCommunityString; }
           set { sCommunityString = value; }
       }

        /// <summary>
       /// RebootPrinter
       /// Cross-Platform method to reboot physical printer devices. Method requires an IP adress or
       /// DNS name of the target. This method should always be in a try/catch bracket. Returns either
       /// true/false (if no error encountered) as to the status of the operation. You may retrieve any
       /// system messages from the RETURN_MESSAGE field.
        /// </summary>
        /// <param name="sTarget"></param>
        /// <returns></returns>
       public static bool RebootPrinter(string sTarget)
       {
           try
           {
               ipaTarget = IPAddress.Parse(sTarget);
           }
           catch (Exception)
           {
               bIPAddress = false;
           }

           if (bIPAddress == true)
           {
               try
               {
                   IPHostEntry ipEntry = Dns.GetHostEntry(sTarget);
                   ipaTarget = IPAddress.Parse(ipEntry.AddressList[0].ToString());
               }
               catch (Exception exa)
               {
                   throw new Exception("Could not retrieve address from entry: " + sTarget + ", reason: " + exa.Message);
               }

               UdpTarget target = new UdpTarget(ipaTarget);
               // Create a SET PDU
               Pdu pdu = new Pdu(PduType.Set);

               // Set the reboot OID to 4 
               pdu.VbList.Add(new Oid("1.3.6.1.2.1.43.5.1.1.3.1"), new Integer32(4));

               // Set Agent security parameters
               AgentParameters aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString(sCommunityString));
               // Response packet
               SnmpV2Packet response;

               try
               {
                   // Send request and wait for response
                   response = target.Request(pdu, aparam) as SnmpV2Packet;
               }
               catch (Exception ex)
               {
                   // Returned errors here...
                   target.Close();
                   throw new Exception(String.Format("Request failed with exception: {0}", ex.Message));
               }

               if (response == null)
               {
                   Console.WriteLine("Error in sending SNMP request.");
               }
               else
               {
                   // Check if we received an SNMP error from the agent
                   if (response.Pdu.ErrorStatus != 0)
                   {
                       RETURN_MESSAGE = String.Format("SNMP agent returned ErrorStatus {0} on index {1}",
                         response.Pdu.ErrorStatus, response.Pdu.ErrorIndex);
                   }
                   else
                   {
                       // Everything is ok. Agent will return the new value for the OID we changed
                       RETURN_MESSAGE = String.Format("Agent response {0}: {1}",
                         response.Pdu[0].Oid.ToString(), response.Pdu[0].Value.ToString());
                   }
                   bReturn = true;
               }
           }
           return bReturn;
       }
    }
}
