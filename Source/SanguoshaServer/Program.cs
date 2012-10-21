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
        static int totalNumberOfPlayers = 3;
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
                proxy.TimeOutSeconds = 15;
                proxy.HostPlayer = player;
                game.UiProxies.Add(player, proxy);
            }
            game.GlobalServerProxy= new GlobalServerUiProxy(game, game.UiProxies);
            game.GlobalServerProxy.TimeOutSeconds = 15;
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
