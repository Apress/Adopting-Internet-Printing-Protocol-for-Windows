using System;
using System.Collections.Generic;
using System.IO;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.PowerPoint;

namespace IPPUtil
{/*------------------------------------------
  ComOfficeConversion

    Class that uses Office conversion routines exposed by COM
    for Word and Excel conversion to PDF.
  ------------------------------------------*/
    class ComOfficeConversion
    {
        public enum OFFICE_TYPE
        {
            WORD = 0,
            EXCEL = 1,
            POWERPOINT = 2,
            UNSUPORTED = 3
        };

        public static OFFICE_TYPE GetOfficeType(string file)
        {
            string extension = Path.GetExtension(file);
            if ((string.Compare(extension, ".doc", true) == 0) || (string.Compare(extension, ".docx", true) == 0))
            {
                return OFFICE_TYPE.WORD;
            }
            else if ((string.Compare(extension, ".xls", true) == 0) || (string.Compare(extension, ".xlsx", true) == 0))
            {
                return OFFICE_TYPE.EXCEL;
            }
            else if ((string.Compare(extension, ".ppt", true) == 0) || (string.Compare(extension, ".pptx", true) == 0))
            {
                return OFFICE_TYPE.UNSUPORTED;
            }
            else
                return OFFICE_TYPE.UNSUPORTED;
        }


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
            else if((string.Compare(extension, ".xls", true) == 0) || (string.Compare(extension, ".xlsx", true) == 0))
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
        /// Converts an Office document to pdf. This method should always be
        /// enclosed in a try/catch statement.
        /// </summary>
        /// <param name="document"></param>
        public static string Convert2Pdf(string officeFile)
        {
            OFFICE_TYPE ot = GetOfficeType(officeFile);

            switch (ot)
            {
                case OFFICE_TYPE.WORD:
                    return DoWordConversion(officeFile);
                case OFFICE_TYPE.EXCEL:
                    return DoExcelConversion(officeFile); 
                default:
                    throw new Exception("Unsupported Office format - cannot convert!");
            }
        }


        /// <summary>
        /// DoWordConversion
        /// </summary>
        /// <param name="officeFile"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string DoWordConversion(string officeFile)
        {
            Word.Application app = null;
            Word.Document doc = null;
            string sNewFile = null;

            try
            {
                app = new Word.Application();
                doc = app.Documents.Open(officeFile);
                //Get new Name
                sNewFile = GetUniqueFilename(ChangeToPdfExtension(officeFile));
                doc.SaveAs2(sNewFile, Word.WdSaveFormat.wdFormatPDF);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to convert {0} to pdf, reason: {1}", officeFile, ex.Message));
            }
            finally
            {
                if (doc != null)
                    doc.Close();
                if (app != null)
                    app.Quit();
            }

            return sNewFile;
        }

        /// <summary>
        /// DoExcelConversion
        /// </summary>
        /// <param name="officeFile"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string DoExcelConversion(string officeFile)
        {
            Excel.Application app = null;
            Excel.Workbook wb = null;
            string sNewFile = null;

            try
            {
                app = new Excel.Application();
                wb = app.Workbooks.Open(officeFile);
                //Get new Name
                sNewFile = GetUniqueFilename(ChangeToPdfExtension(officeFile));
                wb.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, sNewFile);

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to convert {0} to pdf, reason: {1}", officeFile, ex.Message));
            }
            finally
            {
                if (wb != null)
                    wb.Close();
                if (app != null)
                    app.Quit();
            }

            return sNewFile;
        }

        public static string DoPptConversion(string officeFile)
        {
            Microsoft.Office.Interop.PowerPoint.Application app = null;
            Presentation pres = null;
            string sNewFile = null;

            try
            {
                app = new Microsoft.Office.Interop.PowerPoint.Application();
                //pres = app.Presentations.Open(officeFile);
                //Get new Name
                sNewFile = GetUniqueFilename(ChangeToPdfExtension(officeFile));
                //pres.ExportAsFixedFormat(sNewFile, PpFixedFormatType.ppFixedFormatTypePDF);

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to convert {0} to pdf, reason: {1}", officeFile, ex.Message));
            }
            finally
            {
                if (pres != null)
                    pres.Close();
                if (app != null)
                    app.Quit();
            }

            return sNewFile;
        }

    }
}

