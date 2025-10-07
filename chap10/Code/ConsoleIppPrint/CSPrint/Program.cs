using System;
using System.Drawing.Printing;
using PdfiumViewer;
using System.IO;

namespace CSPrint
{
    internal class Program
    {
        static string sPrinterName = string.Empty;
        static string sPdfFilePath = string.Empty;
        static void Main(string[] args)
        {
            GetArguments();
            if (File.Exists(sPdfFilePath) == false)
            {
                Console.WriteLine($"Error, could not find file: {sPdfFilePath}");
                return;
            }

            Console.WriteLine($"Please wait to print {sPdfFilePath} on {sPrinterName}...");
            try
            {
                using (var document = PdfDocument.Load(sPdfFilePath))
                {
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.PrinterSettings.PrinterName = sPrinterName;
                        printDocument.PrintController = new StandardPrintController();
                        printDocument.PrinterSettings.Duplex = Duplex.Vertical;
                        printDocument.PrinterSettings.Copies = 1;
                        printDocument.Print();
                    }
                }

                Console.WriteLine("Printed successfully.");
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"Error attempting to print {sPdfFilePath} on  {sPrinterName}: {ex.ToString()}");
                return;
            }
        }

        /// <summary>
        /// GetArguments
        /// 
        /// Ask the user for printer and file
        /// </summary>
        static void GetArguments()
        {
            while (true)
            {
                Console.WriteLine("Enter the printer to use: ");

                sPrinterName = Console.ReadLine().Trim();

                if (sPrinterName.Length > 0)
                {
                    Console.WriteLine($"You chose to use {sPrinterName}");
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid response - you must provide a printer to use..");
                }
            }

            while (true)
            {
                Console.WriteLine("Enter the pdf file and path to print: ");

                sPdfFilePath = Console.ReadLine().Trim();

                if (sPdfFilePath.Length > 0)
                {
                    Console.WriteLine($"You chose to use {sPdfFilePath}");
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid response - you must provide a pdf file to print..");
                }
            }
        }

    }
}
