/**
 *  Copyright (C) 2006 Alex Pedenko
 * 
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.IO;
namespace NetSync
{
    public class Util
    {
        static bool pushDirInitialized = false;
        public static string currDir = null;

        /// <summary>
        /// Checks the file 'mode' to see whether the file is a directory. If so it returns True
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static bool S_ISDIR(uint mode)
        {
            return (mode & 0x8000) == 0;
        }

        /// <summary>
        /// Just return first parameter
        /// </summary>
        /// <param name="p"></param>
        /// <param name="rootDir"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static string sanitizePath(string p, string rootDir, int depth) //@todo_long remove it if possible
        {
            return p;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static bool pushDir(string dir)
        {
            if (!pushDirInitialized)
            {
                pushDirInitialized = true;
                // TODO: path length
                currDir = Directory.GetCurrentDirectory();
            }

            //if (string.IsNullOrEmpty(dir)) // We can skip this check because try will catch this situation
            //{
            //    return false;
            //}
            //...
            try
            {
                // TODO: path length
                Directory.SetCurrentDirectory(dir);
            }
            catch
            {
                return false;
            }

            // TODO: path length
            currDir = Directory.GetCurrentDirectory();
            return true;
        }

        /// <summary>
        /// Cd given 'dir' if possible
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static bool popDir(string dir)
        {
            try
            {
                // TODO: path length
                Directory.SetCurrentDirectory(dir);
            }
            catch
            {
                return false;
            }
            currDir = dir;
            return true;
        }

        /// <summary>
        /// If relative filename or dirname is given then convert it to absolute
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string fullFileName(string fileName)
        {
            //...Safe FileName
            if (fileName.IndexOf(':') != -1 || fileName.StartsWith(@"\\"))
            {
                return fileName;	//absolute
            }
            else
            {
                return currDir + @"\" + fileName;	//relative
            }
            //...modules
        }

        /// <summary>
        /// Replaces "\\" by "\", then removes trailing "\" .
        /// Also if collapseDotDot is true then removes all ".." except for first two symbols of filename.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="collapseDotDot"></param>
        /// <returns></returns>
        public static string cleanFileName(string fileName, bool collapseDotDot)
        {
            string cleanedName = fileName.Replace(@"\\", @"\");
            //if (cleanedName.EndsWith(@"\")) //@fixed possibly replace by TrimEnd?
            //{
            //    cleanedName = cleanedName.Substring(0, cleanedName.Length - 1);
            //}
            cleanedName = cleanedName.TrimEnd('\\');
            if (collapseDotDot)
            {
                cleanedName = cleanedName.Substring(0, 2) + cleanedName.Substring(2).Replace("..", String.Empty);
            }
            return cleanedName;
        }

        /// <summary>
        /// Returns "<NULL>", if 's' is null, otherwise returns 's' itself
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string NS(string s)
        {
            return s == null ? "<NULL>" : s;
        }

        /// <summary>
        /// Find position of ":" taking into account "/"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int FindColon(string s)
        {
            int index = s.IndexOf(":");
            if (index == -1)
            {
                return -1;
            }
            int slashIndex = s.IndexOf("/");
            if (slashIndex != -1 && slashIndex < index)
            {
                return -1;
            }
            return index;
        }

        /// <summary>
        /// Returns array without last element
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string[] DeleteLastElement(string[] x)
        {
            string[] y = new string[x.Length - 1];
            for (int i = 0; i < y.Length; i++)
            {
                y[i] = x[i];
            }
            return y;
        }

        /// <summary>
        /// Returns array without first element
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string[] DeleteFirstElement(string[] x)
        {
            string[] y = new string[x.Length - 1];
            for (int i = 0; i < y.Length; i++)
            {
                y[i] = x[i + 1];
            }
            return y;
        }

        /// <summary>
        /// Compares two arrays of bytes and return 0, if they equal at intersing positions, otherwise return difference of first
        /// not equal bytes (arr1[j]-arr2[j])
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="off1"></param>
        /// <param name="arr2"></param>
        /// <param name="off2"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int MemoryCompare(byte[] arr1, int off1, byte[] arr2, int off2, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (arr1[off1 + i] != arr2[off2 + i])
                {
                    return arr1[off1 + i] - arr2[off2 + i];
                }
            }
            return 0;
        }

        /// <summary>
        /// Copies 'count' bytes from 'source' starting at 'sourceOffset' to 'dest' starting at 'destOffset'
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="destOffset"></param>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="count"></param>
        public static void MemoryCopy(byte[] dest, int destOffset, byte[] source, int sourceOffset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                dest[destOffset + i] = source[sourceOffset + i];
            }
        }

        /// <summary>
        /// Compares time of modification for given files
        /// </summary>
        /// <param name="file1ModificationTime"></param>
        /// <param name="file2ModificationTime"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static int CompareModificationTime(long file1ModificationTime, long file2ModificationTime, Options options)
        {
            if (file2ModificationTime > file1ModificationTime)
            {
                if (file2ModificationTime - file1ModificationTime <= options.modifyWindow)
                {
                    return 0;
                }
                return -1;
            }
            if (file1ModificationTime - file2ModificationTime <= options.modifyWindow)
            {
                return 0;
            }
            return 1;
        }
    }
}
