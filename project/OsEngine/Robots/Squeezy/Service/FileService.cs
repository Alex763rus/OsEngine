using OsEngine.Logging;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service
{
    public class FileService
    {
        public static void saveMessageInFile(string filePath, string data, bool append)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, append))
                {
                    writer.WriteLine(data);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                //skip
            }
        }
        public static void deleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                //skip
            }
        }

    }
}
