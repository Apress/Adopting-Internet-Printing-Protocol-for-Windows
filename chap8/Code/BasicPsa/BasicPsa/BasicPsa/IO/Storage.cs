using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Graphics.Printing.PrintTicket;
using Windows.Storage;

namespace BasicPsa
{
    /// <summary>
    /// Storage
    /// 
    /// Provides rudimentary file storage/retrieval to the local folder for this deployed app.
    /// 
    /// When a UWP app is installed, several folders are created under c:\users\<user name>\AppData\Local\Packages\<app package identifier>\ to store, 
    /// the app's local, roaming, and temporary files. The app doesn't need to declare any capabilities to access these folders, 
    /// and these folders are not accessible by other apps. These folders are also removed when the app is uninstalled.
    /// </summary>
    public class Storage
    {
        public static async Task WriteSettings(List<string> appSettings)
        {
            try
            {
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile file = await storageFolder.CreateFileAsync("BasicPsaStorage.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteLinesAsync(file, appSettings);
                // Log the written items
                System.Diagnostics.Debug.WriteLine("Written items: " + string.Join(", ", appSettings));
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                System.Diagnostics.Debug.WriteLine("Error writing settings: " + ex.Message);
            }
        }

        public static async Task<List<string>> GetSettings()
        {
            List<string> content = new List<string>();
            try
            {
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile file = await storageFolder.GetFileAsync("BasicPsaStorage.txt");
                IList<string> items = await Windows.Storage.FileIO.ReadLinesAsync(file);
                content = items.Select(x => x.ToLower()).ToList();
                // Log the read items
                System.Diagnostics.Debug.WriteLine("Read items: " + string.Join(", ", content));
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file does not exist
                System.Diagnostics.Debug.WriteLine("File not found.");
            }
            catch (Exception ex)
            {
                // Handle other exceptions (e.g., log them)
                System.Diagnostics.Debug.WriteLine("Error reading settings: " + ex.Message);
            }
            return content;
        }
    }

    public static class PrintTicketStorage
    {
        public static async Task StorePrintTicketOptionsAsync(List<PrintTicketOption> printTicketOptions)
        {
            // Get the local folder
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            // Create or replace the file
            StorageFile file = await localFolder.CreateFileAsync("PrintTicketOptionStorage.txt", CreationCollisionOption.ReplaceExisting);

            // Serialize the list of PrintTicketOptions to JSON and write to the file
            string jsonString = JsonSerializer.Serialize(printTicketOptions);
            await FileIO.WriteTextAsync(file, jsonString);
        }

        public static async Task<List<PrintTicketOption>> RetrievePrintTicketOptionsAsync()
        {
            // Get the local folder
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            // Get the file
            StorageFile file = await localFolder.GetFileAsync("PrintTicketOptionStorage.txt");

            // Read the JSON string from the file and deserialize it to a list of PrintTicketOptions
            string jsonString = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<List<PrintTicketOption>>(jsonString);
        }
    }
}
