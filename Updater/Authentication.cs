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
using System.Text;

namespace NetSync
{
    class Authentication
    {
        public static string password_file = String.Empty;

        /// <summary>
        /// Encodes message, treated as ASCII, into base64 string
        /// </summary>
        /// <param name="message">ASCII string</param>
        /// <returns></returns>
        public static string Base64Encode(string message)
        {
            Encoding asciiEncoding = Encoding.ASCII;
            byte[] byteArray = new byte[asciiEncoding.GetByteCount(message)];
            byteArray = asciiEncoding.GetBytes(message);
            return Convert.ToBase64String(byteArray);
        }

        /// <summary>
        /// Generates challenge using time as vector
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static string GenerateChallenge(string addr, Options opt)
        {
            string challenge = String.Empty;
            byte[] input = new byte[32];
            DateTime timeVector = DateTime.Now;

            for (int i = 0; i < addr.Length; i++)
            {
                input[i] = Convert.ToByte(addr[i]);
            }

            CheckSum.SIVAL(ref input, 16, (UInt32)timeVector.Second);
            CheckSum.SIVAL(ref input, 20, (UInt32)timeVector.Hour);
            CheckSum.SIVAL(ref input, 24, (UInt32)timeVector.Day);

            Sum sum = new Sum(opt);
            sum.Init(0);
            sum.Update(input, 0, input.Length);
            challenge = Encoding.ASCII.GetString(sum.End());
            return challenge;
        }

        /// <summary>
        /// Generate hash for passwords
        /// </summary>
        /// <param name="indata"></param>
        /// <param name="challenge"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GenerateHash(string indata, string challenge, Options options)
        {
            Sum sum = new Sum(options);

            sum.Init(0);
            sum.Update(Encoding.ASCII.GetBytes(indata), 0, indata.Length);
            sum.Update(Encoding.ASCII.GetBytes(challenge), 0, challenge.Length);
            byte[] buf = sum.End();
            string hash = Convert.ToBase64String(buf);
            return hash.Substring(0, (buf.Length * 8 + 5) / 6);
        }

        /// <summary>
        /// Client-side authorization request maker
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="challenge"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string AuthorizeClient(string user, string pass, string challenge, Options options)
        {
            if (String.Empty.Equals(user))
            {
                user = "nobody";
            }
            if (string.IsNullOrEmpty(pass))
            {
                pass = GetEmptyPassword(password_file);
            }
            if (pass.Equals(String.Empty))
            {
                pass = System.Environment.GetEnvironmentVariable("RSYNC_PASSWORD");
            }
            if (string.IsNullOrEmpty(pass))
            {
                pass = GetPassword();
            }
            string pass2 = GenerateHash(pass, challenge, options);
            Log.WriteLine(user + " " + pass2);

            return pass2;
        }

        /// <summary>
        /// Request a password from user by Console
        /// </summary>
        /// <returns></returns>
        public static string GetPassword()
        {
            Console.Write("Password: ");
            return Console.ReadLine();
        }

        /// <summary>
        /// Return String.Empty in any case
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetEmptyPassword(string filename)
        {
            return String.Empty; //@fixed what is the goal of this method? ok, just empty pass
        }

        /// <summary>
        /// Server-side authorization check
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="moduleNumber"></param>
        /// <param name="addr"></param>
        /// <param name="leader"></param>
        /// <returns></returns>
        public static bool AuthorizeServer(ClientInfo clientInfo, int moduleNumber, string addr, string leader)
        {
            string users = Daemon.config.GetAuthUsers(moduleNumber).Trim();
            //string challenge;
            string b64Challenge;
            IOStream ioStream = clientInfo.IoStream;
            string line;

            string user = String.Empty;
            string secret = String.Empty;
            string pass = String.Empty;
            string pass2 = String.Empty;
            string[] listUsers;
            string token = String.Empty;

            /* if no auth list then allow anyone in! */
            if (string.IsNullOrEmpty(users))
            {
                return true;
            }

            b64Challenge = Base64Encode(GenerateChallenge(addr, clientInfo.Options));
            ioStream.IOPrintf(leader + b64Challenge + "\n");

            line = ioStream.ReadLine();

            if (line.IndexOf(' ') > 0)
            {
                user = line.Substring(0, line.IndexOf(' '));
                pass = line.Substring(line.IndexOf(' ')).Trim('\n').Trim();
            }
            else
            {
                return false;
            }
            listUsers = users.Split(',');

            for (int i = 0; i < listUsers.Length; i++)
            {
                token = listUsers[i];
                if (user.Equals(token))
                {
                    break;
                }
                token = null;
            }

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            if ((secret = GetSecret(moduleNumber, user)) == null)
            {
                return false;
            }

            pass2 = GenerateHash(secret, b64Challenge, clientInfo.Options);

            if (pass.Equals(pass2))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets secret for given Module(by module number) and user
        /// </summary>
        /// <param name="moduleNumber"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        static string GetSecret(int moduleNumber, string user)
        {
            //if (fname == null || fname.CompareTo(String.Empty) == 0) //@fixed Why check if next there is a try/catch and also Path.combine won't return null or emprty string
            //{
            //    return null;
            //}
            try
            {
                string fileName = Path.Combine(Environment.SystemDirectory, Daemon.config.GetSecretsFile(moduleNumber));
                string secret = null;
                using (var streamReader = new System.IO.StreamReader(fileName))
                {
                    while (true)
                    {
                        string line = streamReader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        line.Trim();
                        if (!line.Equals(String.Empty) && line[0] != ';' && line[0] != '#')
                        {
                            string[] userp = line.Split(':');
                            if (userp[0].Trim().Equals(user))
                            {
                                secret = userp[1].Trim();
                                break;
                            }
                        }
                    }
                    streamReader.Close();
                    return secret;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
