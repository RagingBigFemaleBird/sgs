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
            for (int i = 0; i < 8; i++)
            {
                var player = new Player();
                player.Id = i;
                game.Players.Add(player);
                IUiProxy proxy = new ConsoleUiProxy();
                proxy.HostPlayer = player;
                game.UiProxies.Add(player, proxy);
            }
            game.GlobalProxy = new ConsoleUiProxy();
            GameEngine.LoadExpansions("Expansions");
            foreach (var g in GameEngine.Expansions.Values)
            {
                game.LoadExpansion(g);
            }
            Server s = new Server(1);
            s.Ready();
            Client c = new Client();
            c.Start();
            game.Run();
        }
    }
}
