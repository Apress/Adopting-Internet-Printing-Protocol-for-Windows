using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
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
            catch(Exception e) 
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
            Console.WriteLine("Would you like to validate, create, cancel, senddoc, or printjob?");
            string sAction = Console.ReadLine().ToUpper();
 
            if (sAction == "VALIDATE")
            {
                //----------------------------Validate-Job-------------------------
                try
                {
                    ValidateJobRequest vjr = new ValidateJobRequest("1.1", sIppPrinter, false, request, jobAttributes);
                    CompletionStruct cs = await vjr.SendRequestAsync();
                    if(cs.status == 0)
                        Console.WriteLine($"Job successfully validated on {sIppPrinter}");
                    else
                        Console.WriteLine($"Job validation failed, status returned was {cs.status}");
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
                    if (cs.status == 0)
                    {
                        Console.WriteLine($"Job successfully created on {sIppPrinter}");
                        Console.WriteLine("Job Id is: " + cs.jobId.ToString());
                    }
                    else
                        Console.WriteLine($"Job validation failed, status returned was {cs.status}");
                   
                
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
                    Console.WriteLine("Enter the job id: ");
                    int jobId = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("Enter the file to print: ");
                    string file = Console.ReadLine();
                    SendDocRequest sdr = new SendDocRequest("1.1", sIppPrinter, false, request + 1, jobId, file);
                    CompletionStruct cs = await sdr.SendRequestAsync();
                    if (cs.status == 0)
                    {
                        Console.WriteLine($"Send-Document successful on {sIppPrinter} for job: {jobId}");
                    }
                    else
                        Console.WriteLine($"Send-Document failed, status returned was {cs.status}");
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
                    if (cs.status == 0)
                    {
                        Console.WriteLine($"Job {jobId} successfully cancelled!");
                    }
                    else
                    {
                        Console.WriteLine($"Cancel Job failed, return status was: {cs.status}");
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
                    PrintJobRequest pjr = new PrintJobRequest("1.1", sIppPrinter, false, request, file, jobAttributes);
                    CompletionStruct cs = await pjr.SendRequestAsync();
                    if (cs.status == 0)
                    {
                        Console.WriteLine($"Print-Job request successful!");
                    }
                    else
                    {
                        Console.WriteLine($"Print submission failed, return status was: {cs.status}");
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
            else 
            {
                Console.WriteLine("Action requested invalid!");
            }

            Console.WriteLine("IPP print request completed");
        }
    }
}
