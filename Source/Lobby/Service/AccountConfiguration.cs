using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Lobby.Core
{
    [Serializable]
    public class AccountConfiguration
    {
        public List<LoginToken> LoginTokens { get; private set; }
        public List<Account> Accounts { get; private set; }
        public List<bool> IsDead { get; private set; }
        public AccountConfiguration()
        {
            LoginTokens = new List<LoginToken>();
            Accounts = new List<Account>();
            IsDead = new List<bool>();
        }
    }
}
