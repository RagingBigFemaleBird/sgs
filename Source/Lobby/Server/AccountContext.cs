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
        public AccountContext()
            : base(@"data source=(LocalDB)\v11.0; 
                 initial catalog=users;
                 integrated security=true")
        {
        }
        public DbSet<Account> Accounts { get; set; }
    }
}
