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
    public class ClientServer
    {
        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="f"></param>
        public static void SendListing(IOStream f) //@todo_long empty method
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="moduleNumber"></param>
        /// <returns></returns>
        public static void RsyncModule(ClientInfo clientInfo, int moduleNumber) //@fixed Why return something if result not used?
        {
            string path = Daemon.config.GetModule(moduleNumber).Path;
            string name = Daemon.config.GetModuleName(moduleNumber);
            IOStream ioStream = clientInfo.IoStream;
            Options options = clientInfo.Options;
            string[] args = new string[Options.MAX_ARGS];
            int argc = 0, maxArgs = Options.MAX_ARGS;
            string line = String.Empty;

            if (path.Length > 0 && path[0] == '/')
            {
                path = path.Remove(0, 1);
            }
            path = path.Replace("\n", String.Empty);

            Access ac = new Access();
            if (!ac.AllowAccess(options.remoteAddr, options.remoteHost, Daemon.config.GetHostsAllow(moduleNumber), Daemon.config.GetHostsDeny(moduleNumber)))
            {
                Log.Write("rsync denied on module " + name + " from " + options.remoteHost + " (" + options.remoteAddr + ")");
                ioStream.IOPrintf("@ERROR: access denied to " + name + " from " + options.remoteHost + " (" + options.remoteAddr + ")\n");
                return;
            }

            if (!Authentication.AuthorizeServer(clientInfo, moduleNumber, options.remoteAddr, "@RSYNCD: AUTHREQD "))
            {
                Log.Write("auth failed on module " + name + " from " + options.remoteHost + " (" + options.remoteAddr + ")\n");
                ioStream.IOPrintf("@ERROR: auth failed on module " + name + "\n");
                return;
            }
            // TODO: path length
            if (path.Length == 0 || Directory.Exists(path))
            {
                ioStream.IOPrintf("@RSYNCD: OK\n");
            }
            else
            {
                try
                {
                    // TODO: path length
                    Directory.CreateDirectory(path);
                    ioStream.IOPrintf("@RSYNCD: OK\n");
                }
                catch (Exception)
                {
                    ioStream.IOPrintf("@ERROR: Path not found\n");
                    MainClass.Exit("@ERROR: Path not found: " + path, clientInfo);
                }
            }
            options.amServer = true;	//to fix error in SetupProtocol
            options.dir = path;

            while (true)
            {
                line = ioStream.ReadLine();
                line = line.Substring(0, line.Length - 1);
                if (line.CompareTo(String.Empty) == 0)
                {
                    break;
                }
                if (argc == maxArgs)
                {
                    maxArgs += Options.MAX_ARGS;
                    MapFile.ExtendArray(ref args, maxArgs);
                }
                args[argc++] = line;
            }
            args[argc++] = path;

            options.verbose = 0;
            int argsNotUsed = CommandLineParser.ParseArguments(args, options);
            if (argsNotUsed == -1)
            {
                MainClass.Exit("Error parsing options", clientInfo);
            }
            string[] args2 = new string[argsNotUsed];
            for (int i = 0; i < argsNotUsed; i++)
            {
                args2[i] = args[args.Length - argsNotUsed + i];
            }

            MainClass.SetupProtocol(clientInfo);
            ioStream.IOStartMultiplexOut();
            Daemon.StartServer(clientInfo, args2);
        }
    }
}
