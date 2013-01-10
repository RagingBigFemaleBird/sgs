using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Server
{
    class AccountContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
    }
}
