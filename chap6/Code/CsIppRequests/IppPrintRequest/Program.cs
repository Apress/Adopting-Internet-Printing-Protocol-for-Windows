using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CsIppRequestLib;

namespace IppPrintRequest
{
    public class Program
    {

        static async Task Main(string[] args)
        {
            string sJobAttributesFile;

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: IppPrintRequest <full path to print job attributes file>");
                return;
            }

            try
            {
                sJobAttributesFile = args[0];
            }
            catch (Exception e)
            {
                Console.WriteLine($"Invalid command line, reason {e.Message}");
                return;
            }

            Dictionary<string, List<object>> jobAttributes = null;
            try
            {
                jobAttributes = AttributeHelper.ProcessFile(sJobAttributesFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  
                return;
            }

            int request = 1;
            Console.WriteLine("Enter the IPP printer to use: ");
            string sIppPrinter = Console.ReadLine();
            Console.WriteLine("Would you like to getattributes, validate, create, cancel, senddoc, getjobs, identify, jobattributes, or printjob?");
            string sAction = Console.ReadLine().ToUpper();

            if (sAction == "VALIDATE")
            {
                //----------------------------Validate-Job-------------------------
                try
                {
                    ValidateJobRequest vjr = new ValidateJobRequest("1.1", sIppPrinter, false, request, jobAttributes);
                    CompletionStruct cs = await vjr.SendRequestAsync();
                    if (cs.status <= (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                    {
                        Console.WriteLine("Job attributes successfully validated against printer");
                    }
                    else
                    {
                        Console.WriteLine(Status.GetIppStatusMessage(cs.status));
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else if (sAction == "CREATE")
            {
                //------------------Create-Job--------------------------
                try
                {
                    CreateJobRequest cjr = new CreateJobRequest("1.1", sIppPrinter, false, request, jobAttributes);
                    CompletionStruct cs = await cjr.SendRequestAsync();
                    if (cs.status <= (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                    {
                        Console.WriteLine($"Job successfully created on printer {sIppPrinter}, id of job is: {cs.jobId}");
                    }
                    else
                    {
                        Console.WriteLine($"Job validation failed, status returned was {cs.status}");
                    }

                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else if (sAction == "SENDDOC")
            {
                //------------------Send-Document--------------------------
                try
                {
                    bool bLast = true;
                    Console.WriteLine("Enter the job id: ");
                    int jobId = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("Enter the file to print: ");
                    string file = Console.ReadLine();
                    Console.WriteLine("Is this the last document: Y/N?");
                    string sLast = Console.ReadLine().ToUpper();
                    if (sLast == "N")
                        bLast = false;
                    SendDocRequest sdr = new SendDocRequest("1.1", sIppPrinter, false, request + 1, jobId, file, true, bLast);
                    CompletionStruct cs = await sdr.SendRequestAsync();
                    if (cs.status <= (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                    {
                        Console.WriteLine("Job successfully sent to printer");
                    }
                    else
                    {
                        Console.WriteLine(Status.GetIppStatusMessage(cs.status));
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else if (sAction == "CANCEL")
            {
                //------------------Cancel-Job--------------------------

                try
                {
                    Console.WriteLine("Enter the job id to cancel: ");
                    int jobId = Int32.Parse(Console.ReadLine());
                    CancelJobRequest cajr = new CancelJobRequest("1.1", sIppPrinter, false, request + 1, jobId);
                    CompletionStruct cs = await cajr.SendRequestAsync();
                    if (cs.status <= (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                    {
                        Console.WriteLine("Job successfully completed on printer");
                    }
                    else
                    {
                        Console.WriteLine(Status.GetIppStatusMessage(cs.status));
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else if (sAction == "PRINTJOB")
            {
                //----------------------Print-Job Request------------------------
                try
                {
                    Console.WriteLine("Enter the file to print: ");
                    string file = Console.ReadLine();
                    PrintJobRequest pjr = new PrintJobRequest("1.1", sIppPrinter, false, request, file, jobAttributes, true);
                    CompletionStruct cs = await pjr.SendRequestAsync();
                    Console.WriteLine(Status.GetIppStatusMessage(cs.status));
                    if (cs.status <= (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                    {
                        Console.WriteLine($"Print-Job request successful!");
                    }
                    else
                    {
                        Console.WriteLine($"Print submission failed, return status was: {cs.status}");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else if (sAction == "JOBATTRIBUTES")
            {
                //----------------------Get-Job_Attributes Request------------------------
                try
                {
                    Console.WriteLine("Enter the job id to get attributes: ");
                    int jobId = Int32.Parse(Console.ReadLine());
                    GetJobAttributesRequest gjar = new GetJobAttributesRequest("1.1", sIppPrinter, false, request + 1, jobId);
                    CompletionStruct cs = await gjar.SendRequestAsync();
                    if (cs.status <= (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                    {
                        Console.WriteLine("Job Attributes Request Successful");
                    }
                    else
                    {
                        Console.WriteLine(Status.GetIppStatusMessage(cs.status));
                    }
                }
                catch(Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else if (sAction == "IDENTIFY")
            {
                //----------------------Identify Request------------------------
                try
                {
                    IdentifyPrinterRequest ipr = new IdentifyPrinterRequest("1.1", sIppPrinter, false, request);
                    CompletionStruct cs = await ipr.SendRequestAsync();
                    if (cs.status <= (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                    {
                        Console.WriteLine("Identify request successfully sent to printer");
                    }
                    else
                    {
                        Console.WriteLine(Status.GetIppStatusMessage(cs.status));
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else if (sAction == "GETATTRIBUTES")
            {
                //----------------------Get Printer Attributes Request------------------------
                try
                {
                    GetPrinterAttributesRequest gpa = new GetPrinterAttributesRequest("1.1", sIppPrinter, false, request, true);
                    CompletionStruct cs = await gpa.SendRequestAsync();
                    if (cs.status <= (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                    {
                        Console.WriteLine("Enter an attribute to query, leave blank and Enter for none: ");
                        string atq = Console.ReadLine();
                        if (atq.Length > 0)
                        {
                            List<string> queriedAttributeValues = gpa.GetAttributeValues(atq);
                            foreach (string av in queriedAttributeValues)
                            {
                                Console.WriteLine(av);
                            }
                        }
                        else
                        {
                            IEnumerator<IppAttribute> pas = gpa.GetAttributeValues();
                            while (pas.MoveNext())
                            {
                                IppAttribute pa = pas.Current as IppAttribute;
                                Console.WriteLine(string.Format("{0}:{1}", pa.Name, pa.WriteAttributeValuesString()));
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(Status.GetIppStatusMessage(cs.status));
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            else if (sAction == "GETJOBS")
            {
                try
                {
                    int currentJob = 0, lastJob = 0;
                    //----------------------Get-Jobs Request------------------------
                    bool bCompleted = false, bMyJobsOnly = false;
                    Console.WriteLine("Do you just want to see completed jobs?  T/F ");
                    string bcj = Console.ReadLine().ToUpper();
                    if (bcj == "T")
                        bCompleted = true;
                    else
                        bCompleted = false;
                    Console.WriteLine("Do you want my jobs only?  T/F ");
                    string bmo = Console.ReadLine().ToUpper();
                    if (bmo == "T")
                        bMyJobsOnly = true;
                    else
                        bMyJobsOnly = false;

                    GetJobsRequest gjr = new GetJobsRequest("1.1", sIppPrinter, false, request, bCompleted, bMyJobsOnly);
                    CompletionStruct cs = await gjr.SendRequestAsync();

                    if (cs.status <= (int)Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE)
                    {
                        IEnumerator<IppAttribute> pas = gjr.GetAttributeValues();
                        while (pas.MoveNext())
                        {
                            IppAttribute pa = pas.Current as IppAttribute;
                            currentJob = pa.Index;
                            if (currentJob != lastJob)
                            {
                                Console.WriteLine("-----------------------------------------------");
                            }
                            Console.WriteLine(string.Format("{0}:{1}", pa.Name, pa.WriteAttributeValuesString()));
                            lastJob = currentJob;
                        }
                    }
                    else
                    {
                        Console.WriteLine(Status.GetIppStatusMessage(cs.status));
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("Action requested invalid!");
            }

            Console.WriteLine("IPP print request completed");
        }
    }
}
