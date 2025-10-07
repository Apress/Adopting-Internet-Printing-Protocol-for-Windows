using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CsIppRequestLib;
using Org.BouncyCastle.Asn1.Ocsp;


namespace IppCheck
{
    public class PrinterAttributeQuery
    {
        public const int SUCCESS = 0;

        /// <summary>
        /// PrintTestPageAsync
        /// 
        /// Prints a pre-formed pdf test page
        /// </summary>
        /// <param name="printer"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<int> PrintTestPageAsync(string printer, string file, Dictionary<string, List<object>> jobAttributes)
        {
            int request = 1;
            try
            {
                PrintJobRequest pjr = new PrintJobRequest("1.1", printer, false, request, file, jobAttributes, true);
                CompletionStruct cs = await pjr.SendRequestAsync();
                if (cs.status < (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error printing test page {ex.Message}");
            }
        }
        
        /// <summary>
        /// GetPrinterAttributesAsync
        /// 
        /// Gets IPP attributes for a single printer
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        public static async Task<ObservableCollection<IppPrinter>> GetPrinterAttributesAsync(string _ipp_printer, bool bIpps, string ver)
        {
            int request = 1;
            ObservableCollection<IppPrinter> attList = new ObservableCollection<IppPrinter>();
            try
            {
                IppPrinter _printer = await GetPrinterAttributesRequestAsync(_ipp_printer, request, bIpps, ver);
                attList.Add(_printer);
            }
            catch (Exception ex)
            {
                IppPrinter _printer = new IppPrinter(_ipp_printer);
                _printer.Status = IppPrinter.PrinterIppStatus.UNREACHABLE;
                attList.Add(_printer);
            }
            
            return attList;
        }

        /// <summary>
        /// GetAllPrinterAttributesAsync
        /// 
        /// Gets IPP attributes for a list of printers
        /// </summary>
        /// <param name="printers"></param>
        /// <returns></returns>
        public static async Task<ObservableCollection<IppPrinter>> GetAllPrinterAttributesAsync(List<string> printers, bool Ipps, string ver)
        {
            int request = 1;
            ObservableCollection<IppPrinter> attList = new ObservableCollection<IppPrinter>();
            foreach (string _ipp_printer in printers)
            {
                try
                {
                   IppPrinter _printer = await GetPrinterAttributesRequestAsync(_ipp_printer, request, Ipps, ver);
                   attList.Add(_printer);   
                }
                catch(Exception)
                {
                    IppPrinter _printer = new IppPrinter(_ipp_printer);
                    _printer.Status = IppPrinter.PrinterIppStatus.UNREACHABLE;
                    attList.Add(_printer);
                }
            }
            return attList;
        }

        /// <summary>
        /// ConvertIppRequestStatusToReachabilityStatus
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static IppPrinter.PrinterIppStatus ConvertIppRequestStatusToReachabilityStatus(int ret)
        {
            if (ret <= (int)CsIppRequestLib.Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                return IppPrinter.PrinterIppStatus.REACHABLE_CONFIGURED;
            else 
                return IppPrinter.PrinterIppStatus.REACHABLE_NOT_CONFIGURED;
        }


            /// <summary>
            /// GetPrinterAttributesRequestAsync
            /// 
            /// Retrieves IPP printer attributes
            /// </summary>
            /// <param name="printer"></param>
            /// <param name="ai"></param>
            /// <returns></returns>
            private static async Task<IppPrinter> GetPrinterAttributesRequestAsync(string _ipp_printer, int request, bool ipps, string ver)
            {
                try
                {
                    GetIppAttributesRequest gpa = new GetIppAttributesRequest(ver, _ipp_printer, ipps, request);
                    CompletionStruct cs = await gpa.SendRequestAsync();
                    if (cs.status > (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                    {
                        throw new Exception("Error recorded on get-printer-attributes request, IPP status code: {cs.status}");
                    }
                    else
                    {
                        IppPrinter _printer = new IppPrinter(_ipp_printer);
                        _printer.Status = ConvertIppRequestStatusToReachabilityStatus(cs.status);
                        StringBuilder _sb = new StringBuilder();
					    IEnumerator<IppAttribute> pas = gpa.GetAttributeValues();
					    while (pas.MoveNext())
					    {
						    IppAttribute pa = pas.Current as IppAttribute;
						    try
						    {
							    _printer.AddAttribute(pa);
						    }
						    catch (Exception)
						    {
							    continue;
						    }
					    }
					
                        return _printer;
                    }

                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        throw new Exception(ex.InnerException.Message);
                    }
                    else
                    {
                        throw new Exception(ex.Message);
                    }
                }

            }
    }
}
