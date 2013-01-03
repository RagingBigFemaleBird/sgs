using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Sanguosha.Core.Utils
{
    /// <summary>
    /// Maintains a file sequence fileName{Date}{Sequence number} and removes the most out dated file when
    /// number of files exceeds a certain threshold.
    /// </summary>
    /// <remarks>Not thread-safe.</remarks>
    public class FileRotator
    {       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathName">Path to the directory under which the new file is to be created.</param>
        /// <param name="fileName">Common prefix of the file name.</param>
        /// <param name="extension">Extension name of the file. Must starts with "."</param>
        /// <param name="maxAllowance">Maximum number of files allowed before rotation starts. Must be greater or equal to 0.</param>
        /// <returns></returns>
        public static FileStream CreateFile(string pathName, string fileName, string extension, int maxAllowance)
        {
            if (!Directory.Exists(pathName))
            {
                Directory.CreateDirectory(pathName);
            }
            var filePaths = Directory.EnumerateFiles(pathName);

            var suspects = from filePath in filePaths
                           where Path.GetFileName(filePath).ToLower().StartsWith(fileName.ToLower()) &&
                                 filePath.ToLower().EndsWith(extension.ToLower())
                           orderby File.GetCreationTime(filePath)
                           select filePath;
             
            int total = suspects.Count();
            if (total > maxAllowance)
            {
                foreach (var filePath in suspects.Take(total - maxAllowance))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception)
                    {
                    }
                }
            }            
            DateTime dt = DateTime.Now;
            FileStream fs = null;
            try
            {
                string newFile = string.Format("{0}/{1}{2:yyyymmdd}{3}{4}{5}", pathName, fileName, dt,
                                                (int)dt.TimeOfDay.TotalMilliseconds, Process.GetCurrentProcess().Id, extension);
                fs = new FileStream(newFile, FileMode.Create);
            }
            catch (IOException)
            {                   
            }

            return fs;
        }

    }
}
