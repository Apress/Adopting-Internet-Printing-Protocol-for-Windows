
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Printing;


namespace BasicIppPrint
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string printerName = "HPM528"; 

            // Create a PrintDocument object
            PrintDocument printDocument = new PrintDocument();

            // Set the printer name
            printDocument.PrinterSettings.PrinterName = printerName;

            // Attach the PrintPage event handler
            printDocument.PrintPage += new PrintPageEventHandler(PrintPage);

            PrintTicket printTicket = new PrintTicket();
            printTicket.PageResolution = new PageResolution(600, 600);
            printTicket.InputBin = InputBin.AutoSelect;
            printTicket.PageMediaType = PageMediaType.Plain;
            printTicket.Duplexing = Duplexing.TwoSidedLongEdge;

            // Assign the PrintTicket to the print queue
            LocalPrintServer localPrintServer = new LocalPrintServer();
            PrintQueue printQueue = localPrintServer.GetPrintQueue(printerName);
            printQueue.UserPrintTicket = printTicket;

            // Print the document
            printDocument.Print();
            Console.WriteLine("fini");
        
        }

        private static void PrintPage(object sender, PrintPageEventArgs e)
        {
            // Define the content to print
            string text = "This is a test print page.";

            // Set the font and brush
            Font font = new Font("Arial", 12);
            Brush brush = Brushes.Black;

            // Define the rectangle to print within
            RectangleF rect = new RectangleF(50, 50, e.PageBounds.Width - 100, e.PageBounds.Height - 100);

            // Draw the text
            e.Graphics.DrawString(text, font, brush, rect);

        }
    }
}
