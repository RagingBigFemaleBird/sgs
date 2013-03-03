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
using System.Net.Sockets;
using System.Text;

namespace NetSync
{
    public class WinRsync
    {
        [STAThread]
        static void Main(string[] args)
        {
            MainClass mc = new MainClass();
            mc.Run(args);
        }
    }

    public class MainClass
    {
        private const string BACKUP_SUFFIX = "~";
        private const string RSYNC_NAME = "rsync";
        private const string RSYNC_VERSION = "1.0";
        public static Options opt;

        public void Run(string[] args)
        {

            opt = new Options();
            opt.Init();
            if (args.Length == 0)
            {
                Usage();
                MainClass.Exit(String.Empty, null);
            }
            int argsNotUsed = CommandLineParser.ParseArguments(args, opt);
            if (argsNotUsed == -1)
            {
                MainClass.Exit("Error parsing options", null);
            }
            string[] args2 = new string[argsNotUsed];
            for (int i = 0; i < argsNotUsed; i++)
            {
                args2[i] = args[args.Length - argsNotUsed + i];
            }

            if (opt.amDaemon && !opt.amSender)
            {
                Daemon.DaemonMain(opt);
                return;
            }
            ClientInfo cInfo = new ClientInfo();
            cInfo.Options = opt;
            StartClient(args2, cInfo);
            opt.doStats = true;
            cInfo.IoStream = null;
            Report(cInfo);
            Console.Write("Press 'Enter' to exit.");
            Console.Read();
        }


        public static void Report(ClientInfo cInfo)
        {
            IOStream f = cInfo.IoStream;
            Options options = cInfo.Options;

            Int64 totalWritten = Options.stats.totalWritten;
            Int64 totalRead = Options.stats.totalRead;
            if (options.amServer && f != null)
            {
                if (options.amSender)
                {
                    f.WriteLongInt(totalRead);
                    f.WriteLongInt(totalWritten);
                    f.WriteLongInt(Options.stats.totalSize);
                }
                return;
            }
            if (!options.amSender && f != null)
            {
                /* Read the first two in opposite order because the meaning of
                 * read/write swaps when switching from sender to receiver. */
                totalWritten = f.ReadLongInt();
                totalRead = f.ReadLongInt();
                Options.stats.totalSize = f.ReadLongInt();
            }

            if (options.doStats)
            {
                Log.WriteLine("Number of files: " + Options.stats.numFiles);
                Log.WriteLine("Number of files transferred: " + Options.stats.numTransferredFiles);
                Log.WriteLine("Total file size: " + Options.stats.totalSize);
                Log.WriteLine("Total transferred file size: " + Options.stats.totalTransferredSize);
                Log.WriteLine("Literal data: " + Options.stats.literalData);
                Log.WriteLine("Matched data: " + Options.stats.matchedData);
                Log.WriteLine("File list size: " + Options.stats.fileListSize);
                Log.WriteLine("Total bytes written: " + totalWritten);
                Log.WriteLine("Total bytes received: " + totalRead);
            }
        }

        public static int StartClient(string[] args, ClientInfo cInfo)
        {
            Options options = cInfo.Options;
            if (args[0].StartsWith(Options.URL_PREFIX) && !options.readBatch) //source is remote
            {
                string path, user = String.Empty;
                //string host = args[0].Substring(Options.URL_PREFIX.Length, args[0].Length - Options.URL_PREFIX.Length); //@fixed use 1-param version of Substring
                string host = args[0].Substring(Options.URL_PREFIX.Length);
                if (host.LastIndexOf('@') != -1)
                {
                    user = host.Substring(0, host.LastIndexOf('@'));
                    host = host.Substring(host.LastIndexOf('@') + 1);
                }
                else
                {
                    MainClass.Exit("Unknown host", null);
                }
                if (host.IndexOf("/") != -1)
                {
                    path = host.Substring(host.IndexOf("/") + 1);
                    host = host.Substring(0, host.IndexOf("/"));

                }
                else
                {
                    path = String.Empty;
                }
                if (host[0] == '[' && host.IndexOf(']') != -1)
                {
                    host = host.Remove(host.IndexOf(']'), 1);
                    host = host.Remove(host.IndexOf('['), 1);
                }
                if (host.IndexOf(':') != -1)
                {
                    options.rsyncPort = Convert.ToInt32(host.Substring(host.IndexOf(':')));
                    host = host.Substring(0, host.IndexOf(':'));
                }
                string[] newArgs = Util.DeleteFirstElement(args);
                return StartSocketClient(host, path, user, newArgs, cInfo);
            }

            //source is local
            if (!options.readBatch)
            {
                int p = Util.FindColon(args[0]);
                string user = String.Empty;
                options.amSender = true;
                if (args[args.Length - 1].StartsWith(Options.URL_PREFIX) && !options.readBatch)
                {
                    string path;
                    string host = args[args.Length - 1].Substring(Options.URL_PREFIX.Length);
                    if (host.LastIndexOf('@') != -1)
                    {
                        user = host.Substring(0, host.LastIndexOf('@'));
                        host = host.Substring(host.LastIndexOf('@') + 1);
                    }
                    else
                    {
                        MainClass.Exit("Unknown host", null);
                    }
                    if (host.IndexOf("/") != -1)
                    {
                        path = host.Substring(host.IndexOf("/") + 1);
                        host = host.Substring(0, host.IndexOf("/"));

                    }
                    else
                    {
                        path = String.Empty;
                    }
                    if (host[0] == '[' && host.IndexOf(']') != -1)
                    {
                        host = host.Remove(host.IndexOf(']'), 1);
                        host = host.Remove(host.IndexOf('['), 1);
                    }
                    if (host.IndexOf(':') != -1)
                    {
                        options.rsyncPort = Convert.ToInt32(host.Substring(host.IndexOf(':')));
                        host = host.Substring(0, host.IndexOf(':'));
                    }
                    string[] newArgs = Util.DeleteLastElement(args);
                    return StartSocketClient(host, path, user, newArgs, cInfo);
                }
                p = Util.FindColon(args[args.Length - 1]);
                if (p == -1) //src & dest are local
                {
                    /* no realized*/
                }
                else
                    if (args[args.Length - 1][p + 1] == ':')
                    {
                        if (options.shellCmd == null)
                        {
                            return StartSocketClient(args[args.Length - 1].Substring(0, p), args[args.Length - 1].Substring(p + 2), user, args, cInfo);
                        }
                    }
            }
            return 0;
        }

        public static int StartSocketClient(string host, string path, string user, string[] args, ClientInfo cInfo)
        {
            Options options = cInfo.Options;
            if (path.CompareTo(String.Empty) != 0 && path[0] == '/')
            {
                Log.WriteLine("ERROR: The remote path must start with a module name not a /");
                return -1;
            }
            cInfo.IoStream = OpenSocketOutWrapped(host, options.rsyncPort, options.bindAddress);

            if (cInfo.IoStream != null)
            {
                StartInbandExchange(user, path, cInfo, args.Length);
            }

            Client client = new Client();
            return client.ClientRun(cInfo, -1, args);
        }

        public static int StartInbandExchange(string user, string path, ClientInfo cInfo, int argc)
        {
            Options options = cInfo.Options;
            IOStream f = cInfo.IoStream;

            string[] sargs = new string[Options.MAX_ARGS];
            int sargc = options.ServerOptions(sargs);
            sargs[sargc++] = ".";
            //if(path != null && path.Length>0)
            //sargs[sargc++] = path;

            if (argc == 0 && !options.amSender)
            {
                options.listOnly = true;
            }
            if (path[0] == '/')
            {
                Log.WriteLine("ERROR: The remote path must start with a module name");
                return -1;
            }
            f.IOPrintf("@RSYNCD: " + options.protocolVersion + "\n");
            string line = f.ReadLine();
            try
            {
                options.remoteProtocol = Int32.Parse(line.Substring(9, 2));
            }
            catch
            {
                options.remoteProtocol = 0;
            }
            bool isValidstring = line.StartsWith("@RSYNCD: ") && line.EndsWith("\n") && options.remoteProtocol > 0;
            if (!isValidstring)
            {
                f.IOPrintf("@ERROR: protocol startup error\n");
                return -1;
            }
            if (options.protocolVersion > options.remoteProtocol)
            {
                options.protocolVersion = options.remoteProtocol;
            }
            f.IOPrintf(path + "\n");
            while (true)
            {
                line = f.ReadLine();
                if (line.CompareTo("@RSYNCD: OK\n") == 0)
                {
                    break;
                }
                if (line.Length > 18 && line.Substring(0, 18).CompareTo("@RSYNCD: AUTHREQD ") == 0)
                {
                    string pass = String.Empty;
                    if (user.IndexOf(':') != -1)
                    {
                        pass = user.Substring(user.IndexOf(':') + 1);
                        user = user.Substring(0, user.IndexOf(':'));
                    }
                    f.IOPrintf(user + " " + Authentication.AuthorizeClient(user, pass, line.Substring(18).Replace("\n", String.Empty), options) + "\n");
                    continue;
                }

                if (line.CompareTo("@RSYNCD: EXIT\n") == 0)
                {
                    MainClass.Exit("@RSYNCD: EXIT", null);
                }

                if (line.StartsWith("@ERROR: "))
                {
                    MainClass.Exit("Server: " + line.Replace("\n", String.Empty), null);
                }
            }

            for (int i = 0; i < sargc; i++)
            {
                f.IOPrintf(sargs[i] + "\n");
            }
            f.IOPrintf("\n");
            return 0;
        }

        public static IOStream OpenSocketOutWrapped(string host, int port, string bindAddress)
        {
            return OpenSocketOut(host, port, bindAddress);
        }

        public static IOStream OpenSocketOut(string host, int port, string bindAddress)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(host, port);
            }
            catch (Exception)
            {
                MainClass.Exit("Can't connect to server", null);
            }
            IOStream stream = new IOStream(client.GetStream());
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cInfo"></param>
        public static void SetupProtocol(ClientInfo cInfo)
        {
            IOStream f = cInfo.IoStream;
            Options options = cInfo.Options;

            if (options.remoteProtocol == 0)
            {
                if (!options.readBatch)
                {
                    f.writeInt(options.protocolVersion);
                }
                options.remoteProtocol = f.readInt();
                if (options.protocolVersion > options.remoteProtocol)
                {
                    options.protocolVersion = options.remoteProtocol;
                }
            }
            if (options.readBatch && options.remoteProtocol > options.protocolVersion)
            {
                MainClass.Exit("The protocol version in the batch file is too new", null);
            }
            if (options.verbose > 3)
            {
                Log.WriteLine("(" + (options.amServer ? "Server" : "Client") + ") Protocol versions: remote=" + options.remoteProtocol + ", negotiated=" + options.protocolVersion);
            }
            if (options.remoteProtocol < Options.MIN_PROTOCOL_VERSION || options.remoteProtocol > Options.MAX_PROTOCOL_VERSION)
            {
                MainClass.Exit("Protocol version mistmatch", null);
            }
            if (options.amServer)
            {
                if (options.checksumSeed == 0)
                {
                    options.checksumSeed = (int)System.DateTime.Now.Ticks;
                }
                f.writeInt(options.checksumSeed);
            }
            else
            {
                options.checksumSeed = f.readInt();
            }
        }

        public static void PrintRsyncVersion()
        {
            Log.WriteLine(RSYNC_NAME + " version " + RSYNC_VERSION);
            Log.WriteLine(@"
   This port is Copyright (C) 2006 Alex Pedenko, Michael Feingold and Ivan Semenov
  
   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 2 of the License, or
   (at your option) any later version.
 
   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.
 
   You should have received a copy of the GNU General Public License
   along with this program; if not, write to the Free Software
   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA");
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Usage()
        {
            PrintRsyncVersion();

            Log.WriteLine(string.Empty);
            Log.WriteLine("rsync is a file transfer program capable of efficient remote update via a fast differencing algorithm.");

            Log.WriteLine("Usage: rsync [OPTION]... SRC [SRC]... [USER@]HOST:DEST");
            Log.WriteLine("  or   rsync [OPTION]... [USER@]HOST:SRC DEST");
            Log.WriteLine("  or   rsync [OPTION]... SRC [SRC]... DEST");
            Log.WriteLine("  or   rsync [OPTION]... [USER@]HOST::SRC [DEST]");
            Log.WriteLine("  or   rsync [OPTION]... SRC [SRC]... [USER@]HOST::DEST");
            Log.WriteLine("  or   rsync [OPTION]... rsync://[USER@]HOST[:PORT]/SRC [DEST]");
            Log.WriteLine("  or   rsync [OPTION]... SRC [SRC]... rsync://[USER@]HOST[:PORT]/DEST");
            Log.WriteLine("SRC on single-colon remote HOST will be expanded by remote shell");
            Log.WriteLine("SRC on server remote HOST may contain shell wildcards or multiple");
            Log.WriteLine("  sources separated by space as long as they have same top-level");
            Log.WriteLine("Options");
            Log.WriteLine(" -v, --verbose               increase verbosity");
            Log.WriteLine(" -q, --quiet                 decrease verbosity");
            Log.WriteLine(" -c, --checksum              always checksum");
            Log.WriteLine(" -a, --archive               archive mode, equivalent to -rlptgoD (no -H)");
            Log.WriteLine(" -r, --recursive             recurse into directories");
            Log.WriteLine(" -R, --relative              use relative path names");
            Log.WriteLine("     --no-relative           turn off --relative");
            Log.WriteLine("     --no-implied-dirs       don't send implied dirs with -R");
            Log.WriteLine(" -b, --backup                make backups (see --suffix & --backup-dir)");
            Log.WriteLine("     --backup-dir            make backups into this directory");
            Log.WriteLine("     --suffix=SUFFIX         backup suffix (default " + BACKUP_SUFFIX + " w/o --backup-dir)");
            Log.WriteLine(" -u, --update                update only (don't overwrite newer files)");
            Log.WriteLine("     --inplace               update destination files inplace (SEE MAN PAGE)");
            Log.WriteLine(" -K, --keep-dirlinks         treat symlinked dir on receiver as dir");
            Log.WriteLine(" -l, --links                 copy symlinks as symlinks");
            Log.WriteLine(" -L, --copy-links            copy the referent of all symlinks");
            Log.WriteLine("     --copy-unsafe-links     copy the referent of \"unsafe\" symlinks");
            Log.WriteLine("     --safe-links            ignore \"unsafe\" symlinks");
            Log.WriteLine(" -H, --hard-links            preserve hard links");
            Log.WriteLine(" -p, --perms                 preserve permissions");
            Log.WriteLine(" -o, --owner                 preserve owner (root only)");
            Log.WriteLine(" -g, --group                 preserve group");
            Log.WriteLine(" -D, --devices               preserve devices (root only)");
            Log.WriteLine(" -t, --times                 preserve times");
            Log.WriteLine(" -S, --sparse                handle sparse files efficiently");
            Log.WriteLine(" -n, --dry-run               show what would have been transferred");
            Log.WriteLine(" -W, --whole-file            copy whole files, no incremental checks");
            Log.WriteLine("     --no-whole-file         turn off --whole-file");
            Log.WriteLine(" -x, --one-file-system       don't cross filesystem boundaries");
            Log.WriteLine(" -B, --block-size=SIZE       force a fixed checksum block-size");
            Log.WriteLine(" -e, --rsh=COMMAND           specify the remote shell");
            Log.WriteLine("     --rsync-path=PATH       specify path to rsync on the remote machine");
            Log.WriteLine("     --existing              only update files that already exist");
            Log.WriteLine("     --ignore-existing       ignore files that already exist on receiving side");
            Log.WriteLine("     --delete                delete files that don't exist on the sending side");
            Log.WriteLine("     --delete-excluded       also delete excluded files on the receiving side");
            Log.WriteLine("     --delete-after          receiver deletes after transferring, not before");
            Log.WriteLine("     --ignore-errors         delete even if there are I/O errors");
            Log.WriteLine("     --max-delete=NUM        don't delete more than NUM files");
            Log.WriteLine("     --partial               keep partially transferred files");
            Log.WriteLine("     --partial-dir=DIR       put a partially transferred file into DIR");
            Log.WriteLine("     --force                 force deletion of directories even if not empty");
            Log.WriteLine("     --numeric-ids           don't map uid/gid values by user/group name");
            Log.WriteLine("     --timeout=TIME          set I/O timeout in seconds");
            Log.WriteLine(" -I, --ignore-times          turn off mod time & file size quick check");
            Log.WriteLine("     --size-only             ignore mod time for quick check (use size)");
            Log.WriteLine("     --modify-window=NUM     compare mod times with reduced accuracy");
            Log.WriteLine(" -T, --temp-dir=DIR          create temporary files in directory DIR");
            Log.WriteLine("     --compare-dest=DIR      also compare destination files relative to DIR");
            Log.WriteLine("     --link-dest=DIR         create hardlinks to DIR for unchanged files");
            Log.WriteLine(" -P                          equivalent to --partial --progress");
            Log.WriteLine(" -z, --compress              compress file data");
            Log.WriteLine(" -C, --cvs-exclude           auto ignore files in the same way CVS does");
            Log.WriteLine("     --exclude=PATTERN       exclude files matching PATTERN");
            Log.WriteLine("     --exclude-from=FILE     exclude patterns listed in FILE");
            Log.WriteLine("     --include=PATTERN       don't exclude files matching PATTERN");
            Log.WriteLine("     --include-from=FILE     don't exclude patterns listed in FILE");
            Log.WriteLine("     --files-from=FILE       read FILE for list of source-file names");
            Log.WriteLine(" -0, --from0                 all *-from file lists are delimited by nulls");
            Log.WriteLine("     --version               print version number");
            Log.WriteLine("     --blocking-io           use blocking I/O for the remote shell");
            Log.WriteLine("     --no-blocking-io        turn off --blocking-io");
            Log.WriteLine("     --stats                 give some file transfer stats");
            Log.WriteLine("     --progress              show progress during transfer");
            Log.WriteLine("     --log-format=FORMAT     log file transfers using specified format");
            Log.WriteLine("     --password-file=FILE    get password from FILE");
            Log.WriteLine("     --bwlimit=KBPS          limit I/O bandwidth, KBytes per second");
            Log.WriteLine("     --write-batch=FILE      write a batch to FILE");
            Log.WriteLine("     --read-batch=FILE       read a batch from FILE");
            Log.WriteLine(" -h, --help                  show this help screen");
        }


        public static void Exit(string message, ClientInfo clientInfo)
        {
            Log.Write(message);

            if (!opt.amDaemon)
            {
                Console.Read();
                System.Environment.Exit(0);
            }
            else
            {
                if (clientInfo != null && clientInfo.IoStream != null && clientInfo.IoStream.ClientThread != null)
                {
                    clientInfo.IoStream.ClientThread.Abort();
                }
            }
        }

    }

    class Log
    {
        /// <summary>
        /// Writes string to log adding newLine character at the end
        /// </summary>
        /// <param name="str"></param>
        public static void WriteLine(string str)
        {
            LogWrite(str + Environment.NewLine);
        }

        /// <summary>
        /// Writes string to log
        /// </summary>
        /// <param name="str"></param>
        public static void Write(string str)
        {
            LogWrite(str);
        }

        /// <summary>
        /// Empty method at this moment
        /// </summary>
        /// <param name="file"></param>
        /// <param name="initialStats"></param>
        public static void LogSend(FileStruct file, Stats initialStats)
        {
        }

        /// <summary>
        /// Writes string to logFile or to console if client
        /// </summary>
        /// <param name="str"></param>
        private static void LogWrite(string str)
        {

            if (Daemon.ServerOptions != null)
            {
                if (Daemon.ServerOptions.logFile == null)
                {
                    try
                    {
                        Daemon.ServerOptions.logFile = new FileStream(Path.Combine(Environment.SystemDirectory, "rsyncd.log"), FileMode.OpenOrCreate | FileMode.Append, FileAccess.Write);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
                str = "[ " + DateTime.Now + " ] " + str;
                Daemon.ServerOptions.logFile.Write(Encoding.ASCII.GetBytes(str), 0, str.Length); //@todo cyrillic
                Daemon.ServerOptions.logFile.Flush();
            }
            else
            {
                if (!MainClass.opt.amDaemon)
                {
                    Console.Write(str);
                }
            }

        }
    }

    public class SumBuf
    {
        public int offset;
        public UInt32 len;
        public UInt32 sum1;
        public byte flags;
        public byte[] sum2 = new byte[CheckSum.SUM_LENGTH];
    }

    public class SumStruct
    {
        public int fLength;
        public int count;
        public UInt32 bLength;
        public UInt32 remainder;
        public int s2Length;
        public SumBuf[] sums;
    }

    public class FStat
    {
        public long size;
        public System.DateTime mTime;
        public int mode;
        public int uid;
        public int gid;
        public int rdev;
    }

    public class Progress
    {
        public static void ShowProgress(long offset, long size)
        {
        }

        public static void EndProgress(long size)
        {
        }
    }
}
