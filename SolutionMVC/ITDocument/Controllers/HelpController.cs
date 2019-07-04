using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;

namespace ITDocument.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public FileResult Index()
        {
            string filepath = Server.MapPath(ConfigurationManager.AppSettings["HelpFile"]);
            byte[] pdfByte = GetBytesFromFile(filepath);
            return File(pdfByte, "application/pdf");
        }

        public byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)
            FileStream fs = null;
            try
            {
                fs = System.IO.File.OpenRead(fullFilePath);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                return bytes;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }
    }
}