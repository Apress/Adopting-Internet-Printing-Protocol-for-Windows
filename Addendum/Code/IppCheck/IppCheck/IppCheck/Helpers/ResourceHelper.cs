using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IppCheck
{
    public static class ResourceHelper
    {
        public static BitmapImage LoadEmbeddedImage(string resourceName)
        {
            // Get the current assembly
            var assembly = Assembly.GetExecutingAssembly();

            // Build the full resource name (namespace + filename)
            string fullResourceName = $"{assembly.GetName().Name}.{resourceName}";

            // Load the image from the embedded resource
            using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (stream == null)
                {
                    throw new Exception("Resource not found.");
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze(); // Optional: freeze the bitmap for better performance
                return bitmap;
            }
        }

        public static void ListEmbeddedResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                Console.WriteLine(resourceName);
            }
        }
    }
}
