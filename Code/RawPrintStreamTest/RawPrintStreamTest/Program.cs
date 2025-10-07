using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace RawPrintStreamTest
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.Write("Name of port 9100 printer to test: ");
            string printer_name = Console.ReadLine().Trim();

            // Send the print job
            SendPrintJob(printer_name, 9100);
        }

        static void SendPrintJob(string ipAddress, int port)
        {
            try
            {
                // A standard text string, no PCL formatting
                string stdText = "I love classic cars, especially early and late 60s Mustangs and Corvettes\n" +
                    " and Chargers. To me, these represent the epitome of 60's styling.";

                // Set page width to 60 columns and enable word wrapping
                string pclText = "\x1B&l60F\x1B&k2G" + 
                 "\x1B(s1S\x1B I love classic cars, especially early and late 60s\x1B(s0S " +
                 "\x1B(s3BMustangs\x1B(s0B \x1B(s1Sand\x1B(s0S \x1B(s3BCorvettes\x1B(s0B,\n" +
                 "\x1B(s1Sand\x1B(s0S \x1B(s3BChargers\x1B(s0B." +
                 "\x1B(s1S To me, these represent the epitome of 60's styling.\x1B(s0S\n";

                // Encode the standard text to a byte array
                byte[] stdBytes = Encoding.ASCII.GetBytes(stdText);

                // Convert the PCL text to a byte array
                byte[] pclBytes = Encoding.ASCII.GetBytes(pclText);

                Console.WriteLine("The first page of unformatted text...");
                //Send the standard (no PCL) text stream to the printer
                using (TcpClient client = new TcpClient(ipAddress, port))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        stream.Write(stdBytes, 0, stdBytes.Length);
                    }
                }

                Console.Write("This next page will be PCL encoded, letting the printer know what to do. Press 'Y' to continue...");
                string sAnswer = Console.ReadLine().ToUpper().Trim();
                if (sAnswer == "Y")
                {
                    // Send the PCL text to the printer
                    using (TcpClient client = new TcpClient(ipAddress, port))
                    {
                        using (NetworkStream stream = client.GetStream())
                        {
                            stream.Write(pclBytes, 0, pclBytes.Length);
                        }
                    }
                    Console.WriteLine("PCL text sent to the printer.");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
    }
}
