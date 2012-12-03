using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanguosha.Lobby;

namespace Lobby
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new AccountModelContainer1())
            {
                db.Accounts.Add(new Account() { Username = "DaMuBie", PasswordHash = "12345", Credits = 0, DisplayedName = "DaMuBie", Losses = 0, Wins = 0, Quits = 0 });
                db.SaveChanges();
            }
        }
    }
}
