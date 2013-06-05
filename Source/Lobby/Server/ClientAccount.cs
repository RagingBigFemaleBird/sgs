using Sanguosha.Lobby.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Lobby.Server
{
    public class ClientAccount
    {
        public System.ServiceModel.OperationContext OpContext { get; set; }
        public Account Account { get; set; }
        public ServerRoom CurrentRoom { get; set; }
        public ServerRoom CurrentSpectatingRoom { get; set; }
        public IGameClient CallbackChannel { get; set; }
        public LobbyServiceImpl LobbyService { get; set; }
    }
}
