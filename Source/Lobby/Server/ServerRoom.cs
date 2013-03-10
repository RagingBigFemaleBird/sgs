using Sanguosha.Lobby.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Lobby.Server
{
    public class ServerRoom
    {
        public Room Room { get; set; }
        public HashSet<ClientAccount> Spectators { get; private set; }
        public AccountConfiguration GameInfo { get; set; }
        public ServerRoom()
        {
            Spectators = new HashSet<ClientAccount>();
        }
    }
}
