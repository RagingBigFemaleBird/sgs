using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.UI;
using Sanguosha.Core.Network;

namespace Sanguosha
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            Game game = new RoleGame();
            Server server;
            server = new Server(game, 8);
            for (int i = 0; i < 8; i++)
            {
                var player = new Player();
                player.Id = i;
                if (i == 1)
                {
                    player.IsFemale = true;
                }
                else
                {
                    player.IsMale = true;
                }
                game.Players.Add(player);
                IUiProxy proxy;
                proxy = new ServerNetworkUiProxy(server, i);
                proxy.HostPlayer = player;
                game.UiProxies.Add(player, proxy);
            }
            game.GlobalProxy = new ConsoleUiProxy();
            GameEngine.LoadExpansions("Expansions");
            foreach (var g in GameEngine.Expansions.Values)
            {
                game.LoadExpansion(g);
            }
            game.GameServer = server;
            game.GameClient = null;
            game.Slave = false;
            game.Run();
        }
    }
}
