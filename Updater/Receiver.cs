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
using System.Collections.Generic;
using System.IO;

namespace NetSync
{
    public class Receiver
    {
        private Options options;
        private CheckSum checkSum;

        public Receiver(Options opt)
        {
            options = opt;
            checkSum = new CheckSum(options);
        }

        private string LocalizePath(ClientInfo cInfo, string path)
        {
            string normalized = cInfo.Options.dir.Replace('\\', '/').Replace(":", String.Empty).ToLower();
            string ret = String.Empty;
            if (path.ToLower().IndexOf(normalized) != -1)
            {
                ret = path.Substring(path.ToLower().IndexOf(normalized) + normalized.Length).Replace('/', Path.DirectorySeparatorChar);
            }

            if (ret == String.Empty)
            {
                return path.TrimEnd('\\');
            }
            if (ret[0] == Path.DirectorySeparatorChar)
            {
                ret = ret.Substring(1);
            }

            return ret;
        }

        public int ReceiveFiles(ClientInfo clientInfo, List<FileStruct> fileList, string localName)
        {
            FStat st = new FStat();
            FileStruct file;
            IOStream ioStream = clientInfo.IoStream;

            string fileName;
            string fNameCmp = String.Empty, fNameTmp = String.Empty;
            bool saveMakeBackups = options.makeBackups;
            int i, phase = 0;
            bool recv_ok;

            if (options.verbose > 2)
            {
                Log.WriteLine("ReceiveFiles(" + fileList.Count + ") starting");
            }
            while (true)
            {
                i = ioStream.readInt();
                if (i == -1)
                {
                    if (phase != 0)
                    {
                        break;
                    }

                    phase = 1;
                    checkSum.length = CheckSum.SUM_LENGTH;
                    if (options.verbose > 2)
                    {
                        Log.WriteLine("ReceiveFiles phase=" + phase);
                    }
                    ioStream.writeInt(0); //send_msg DONE
                    if (options.keepPartial)
                    {
                        options.makeBackups = false;
                    }
                    continue;
                }

                if (i < 0 || i >= fileList.Count)
                {
                    MainClass.Exit("Invalid file index " + i + " in receiveFiles (count=" + fileList.Count + ")", clientInfo);
                }

                file = (fileList[i]);

                Options.stats.currentFileIndex = i;
                Options.stats.numTransferredFiles++;
                Options.stats.totalTransferredSize += file.length;

                if (localName != null && localName.CompareTo(String.Empty) != 0)
                {
                    fileName = localName;
                }
                else
                {
                    fileName = Path.Combine(options.dir, LocalizePath(clientInfo, file.GetFullName().Replace(":", String.Empty)).Replace("\\", "/"));
                    //fileName = Path.Combine(options.dir, file.FNameTo().Replace(":",String.Empty)).Replace("\\", "/");
                    // TODO: path length
                    if (file.dirName != null)
                    {
                        Directory.CreateDirectory(Path.Combine(options.dir, LocalizePath(clientInfo, file.dirName.Replace(":", String.Empty))).Replace("\\", "/"));
                        Log.WriteLine(Path.Combine(options.dir, file.dirName));
                    }
                    //FileSystem.Directory.CreateDirectory(Path.Combine(options.dir,file.dirName.Replace(":",String.Empty)).Replace("\\", "/"));
                }

                if (options.dryRun)
                {
                    if (!options.amServer && options.verbose > 0)
                    {
                        Log.WriteLine(fileName);
                    }
                    continue;
                }

                if (options.verbose > 2)
                {
                    Log.WriteLine("receiveFiles(" + fileName + ")");
                }

                if (options.partialDir != null && options.partialDir.CompareTo(String.Empty) != 0)
                {
                }
                else
                {
                    fNameCmp = fileName;
                }

                FileStream fd1 = null;
                try
                {
                    fd1 = new FileStream(fNameCmp, FileMode.Open, FileAccess.Read);
                }
                catch (FileNotFoundException)
                {
                    fNameCmp = fileName;
                    try
                    {
                        fd1 = new FileStream(fNameCmp, FileMode.Open, FileAccess.Read);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }
                catch (Exception e)
                {
                    Log.Write(e.Message);
                }
                try
                {
                    FileInfo fi = new FileInfo(fNameCmp);
                    // TODO: path length
                    st.size = fi.Length;
                }
                catch { }

                String tempFileName = getTmpName(fileName);
                FileStream fd2 = null;
                fd2 = new FileStream(tempFileName, FileMode.OpenOrCreate, FileAccess.Write);

                if (!options.amServer && options.verbose > 0)
                {
                    Log.WriteLine(fileName);
                }

                /* recv file data */
                recv_ok = ReceiveData(clientInfo, fNameCmp, fd1, st.size,
                            fileName, fd2, file.length);

                if (fd1 != null)
                {
                    fd1.Close();
                }
                if (fd2 != null)
                {
                    fd2.Close();
                }
                // TODO: path length
                File.Copy(tempFileName, fileName, true);
                // TODO: path length
                File.Delete(tempFileName);
                if (recv_ok || options.inplace)
                {
                    FinishTransfer(fileName, fNameTmp, file, recv_ok);
                }
            }
            options.makeBackups = saveMakeBackups;

            if (options.deleteAfter && options.recurse && localName == null && fileList.Count > 0)
            {
                DeleteFiles(fileList);
            }

            if (options.verbose > 2)
            {
                Log.WriteLine("ReceiveFiles finished");
            }

            return 0;
        }

        public bool ReceiveData(ClientInfo clientInfo, string fileNameR, Stream fdR, long sizeR, string fileName, Stream fd, int totalSize)
        {
            IOStream f = clientInfo.IoStream;
            byte[] fileSum1 = new byte[CheckSum.MD4_SUM_LENGTH];
            byte[] fileSum2 = new byte[CheckSum.MD4_SUM_LENGTH];
            byte[] data = new byte[Match.CHUNK_SIZE];
            SumStruct sumStruct = new SumStruct();
            MapFile mapBuf = null;
            Sender sender = new Sender(options);
            sender.ReadSumHead(clientInfo, ref sumStruct);
            int offset = 0;
            UInt32 len;

            if (fdR != null && sizeR > 0)
            {
                int mapSize = (int)Math.Max(sumStruct.bLength * 2, 16 * 1024);
                mapBuf = new MapFile(fdR, (int)sizeR, mapSize, (int)sumStruct.bLength);
                if (options.verbose > 2)
                {
                    Log.WriteLine("recv mapped " + fileNameR + " of size " + sizeR);
                }
            }
            Sum sum = new Sum(options);
            sum.Init(options.checksumSeed);

            int i;
            Token token = new Token(options);
            while ((i = token.ReceiveToken(f, ref data, 0)) != 0)
            {
                if (options.doProgress)
                {
                    Progress.ShowProgress(offset, totalSize);
                }

                if (i > 0)
                {
                    if (options.verbose > 3)
                    {
                        Log.WriteLine("data recv " + i + " at " + offset);
                    }
                    Options.stats.literalData += i;
                    sum.Update(data, 0, i);
                    if (fd != null && FileIO.WriteFile(fd, data, 0, i) != i)
                    {
                        goto report_write_error;
                    }
                    offset += i;
                    continue;
                }

                i = -(i + 1);
                int offset2 = (int)(i * sumStruct.bLength);
                len = sumStruct.bLength;
                if (i == sumStruct.count - 1 && sumStruct.remainder != 0)
                {
                    len = sumStruct.remainder;
                }

                Options.stats.matchedData += len;

                if (options.verbose > 3)
                {
                    Log.WriteLine("chunk[" + i + "] of size " + len + " at " + offset2 + " offset=" + offset);
                }

                byte[] map = null;
                int off = 0;
                if (mapBuf != null)
                {
                    off = mapBuf.MapPtr(offset2, (int)len);
                    map = mapBuf.p;

                    token.SeeToken(map, offset, (int)len);
                    sum.Update(map, off, (int)len);
                }

                if (options.inplace)
                {
                    if (offset == offset2 && fd != null)
                    {
                        offset += (int)len;
                        if (fd.Seek(len, SeekOrigin.Current) != offset)
                        {
                            MainClass.Exit("seek failed on " + Util.fullFileName(fileName), clientInfo);
                        }
                        continue;
                    }
                }
                if (fd != null && FileIO.WriteFile(fd, map, off, (int)len) != (int)len)
                {
                    goto report_write_error;
                }
                offset += (int)len;
            }

            if (options.doProgress)
            {
                Progress.EndProgress(totalSize);
            }
            if (fd != null && offset > 0 && FileIO.SparseEnd(fd) != 0)
            {
                MainClass.Exit("write failed on " + Util.fullFileName(fileName), clientInfo);
            }

            fileSum1 = sum.End();

            if (mapBuf != null)
            {
                mapBuf = null;
            }

            fileSum2 = f.ReadBuffer(CheckSum.MD4_SUM_LENGTH);
            if (options.verbose > 2)
            {
                Log.WriteLine("got fileSum");
            }
            if (fd != null && Util.MemoryCompare(fileSum1, 0, fileSum2, 0, CheckSum.MD4_SUM_LENGTH) != 0)
            {
                return false;
            }
            return true;
        report_write_error:
            {
                MainClass.Exit("write failed on " + Util.fullFileName(fileName), clientInfo);
            }
            return true;
        }

        public static void FinishTransfer(string fileName, string fileNameTmp, FileStruct file, bool okToSetTime)
        {
        }

        public void deleteOne(string fileName, bool isDir)
        {
            SysCall sc = new SysCall(options);
            if (!isDir)
            {

                if (!sc.robustUnlink(fileName))
                {
                    Log.WriteLine("Can't delete '" + fileName + "' file");
                }
                else
                {
                    if (options.verbose > 0)
                    {
                        Log.WriteLine("deleting file " + fileName);
                    }
                }
            }
            else
            {
                if (!sc.doRmDir(fileName))
                {
                    Log.WriteLine("Can't delete '" + fileName + "' dir");
                }
                else
                {
                    if (options.verbose > 0)
                    {
                        Log.WriteLine("deleting directory " + fileName);
                    }
                }
            }
        }

        public bool isBackupFile(string fileName)
        {
            return fileName.EndsWith(options.backupSuffix);
        }

        public string getTmpName(string fileName)
        {
            return Path.GetTempFileName();
        }

        public void DeleteFiles(List<FileStruct> fileList)
        {
            string[] argv = new string[1];
            List<FileStruct> localFileList = null;
            if (options.cvsExclude)
            {
                Exclude.AddCvsExcludes();
            }
            for (int j = 0; j < fileList.Count; j++)
            {
                if ((fileList[j].mode & Options.FLAG_TOP_DIR) == 0 || !Util.S_ISDIR(fileList[j].mode))
                {
                    continue;
                }
                argv[0] = options.dir + fileList[j].GetFullName();
                FileList fList = new FileList(options);
                if ((localFileList = fList.sendFileList(null, argv)) == null)
                {
                    continue;
                }
                for (int i = localFileList.Count - 1; i >= 0; i--)
                {
                    if (localFileList[i].baseName == null)
                    {
                        continue;
                    }
                    localFileList[i].dirName = localFileList[i].dirName.Substring(options.dir.Length);
                    if (FileList.fileListFind(fileList, (localFileList[i])) < 0)
                    {
                        localFileList[i].dirName = options.dir + localFileList[i].dirName;
                        deleteOne(localFileList[i].GetFullName(), Util.S_ISDIR(localFileList[i].mode));
                    }
                }
            }
        }
    }



    public class SysCall
    {

        private Options options;

        public SysCall(Options opt)
        {
            options = opt;
        }

        public bool doRmDir(string pathName)
        {
            if (options.dryRun)
            {
                return true;
            }
            if (options.readOnly || options.listOnly)
            {
                return false;
            }
            try
            {
                // TODO: path length
                Directory.Delete(pathName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool robustUnlink(string fileName)
        {
            return doUnlink(fileName);
        }

        public bool doUnlink(string fileName)
        {
            if (options.dryRun)
            {
                return true;
            }
            if (options.readOnly || options.listOnly)
            {
                return false;
            }
            try
            {
                // TODO: path length
                File.Delete(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
