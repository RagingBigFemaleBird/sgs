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
        static int totalNumberOfPlayers = 2;
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            Game game = new RoleGame();
            Server server;
            server = new Server(game, totalNumberOfPlayers);
            for (int i = 0; i < totalNumberOfPlayers; i++)
            {
                var player = new Player();
                player.Id = i;
                game.Players.Add(player);
                IUiProxy proxy;
                proxy = new ServerNetworkUiProxy(server, i);
                proxy.TimeOutSeconds = 25;
                proxy.HostPlayer = player;
                game.UiProxies.Add(player, proxy);
            }
            GlobalServerUiProxy pxy = new GlobalServerUiProxy(game, game.UiProxies);
            pxy.TimeOutSeconds = 25;
            game.GlobalProxy = pxy;
            GameEngine.LoadExpansions("./");
            foreach (var g in GameEngine.Expansions.Values)
            {
                game.LoadExpansion(g);
            }
            game.GameServer = server;
            game.GameClient = null;
            game.IsSlave = false;
            game.Run();
        }
    }
}
