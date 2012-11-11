using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanguosha.Lobby
{
    public class Account
    {
        public Account()
        {
            enabledHeroes = new List<string>();
        }

        string username;

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        string passwordHash;

        public string PasswordHash
        {
            get { return passwordHash; }
            set { passwordHash = value; }
        }

        List<string> enabledHeroes;

        public List<string> EnabledHeroes
        {
            get { return enabledHeroes; }
        }
    }
}
