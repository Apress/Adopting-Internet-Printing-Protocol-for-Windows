using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Spire.Doc;
using Spire.Doc.Documents;

namespace IPPUtil
{
    public static class SpireOfficeConversion
    {
        /// <summary>
        /// IsConvertableDocument
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsConvertibleDocument(string file)
        {
            string extension = Path.GetExtension(file);
            if ((string.Compare(extension, ".doc", true) == 0) || (string.Compare(extension, ".docx", true) == 0))
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordFile"></param>
        /// <returns></returns>
        public static string ChangeToPdfExtension(string officeFile)
        {
            return Path.ChangeExtension(officeFile, "pdf");
        }

        /// <summary>
        /// GetUniqueFilename
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetUniqueFilename(string fullPath)
        {
            if (!Path.IsPathRooted(fullPath))
            {
                fullPath = Path.GetFullPath(fullPath);
            }
            if (File.Exists(fullPath))
            {
                String filename = Path.GetFileName(fullPath);
                String path = fullPath.Substring(0, fullPath.Length - filename.Length);
                String filenameWOExt = Path.GetFileNameWithoutExtension(fullPath);
                String ext = Path.GetExtension(fullPath);
                int n = 1;
                do
                {
                    fullPath = Path.Combine(path, String.Format("{0} ({1}){2}", filenameWOExt, (n++), ext));
                }
                while (File.Exists(fullPath));
            }
            return fullPath;
        }

        /// <summary>
        /// Convert2Pdf
        /// 
        /// Converts a Word document to pdf. This method should always be
        /// enclosed in a try/catch statement.
        /// </summary>
        /// <param name="document"></param>
        public static string Convert2Pdf(string officeFile)
        {
            try
            {
                Document document = new Document();
                document.LoadFromFile(officeFile);

                //Get new Name
                string sNewFile = GetUniqueFilename(ChangeToPdfExtension(officeFile));

                //Convert Word to PDF
                document.SaveToFile(sNewFile, FileFormat.PDF);

                return sNewFile;
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Failed to convert {0} to pdf, reason: {1}", officeFile, ex.Message));
            }
        }
    }
}
