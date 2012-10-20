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
            Console.WriteLine("Who are you?");
            string ids = Console.ReadLine();
            int id = int.Parse(ids);
            Game game = new RoleGame();
            Client client;
            client = new Client();
            client.Start();
            client.SelfId = id;
            for (int i = 0; i < 3; i++)
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
                proxy = new ConsoleUiProxy();
                proxy.HostPlayer = player;
                if (i == id)
                {
                    proxy = new ClientNetworkUiProxy(proxy, client, true);
                    proxy.HostPlayer = player;
                }
                else
                {
                    proxy = new ClientNetworkUiProxy(proxy, client, false);
                    proxy.HostPlayer = player;
                }
                game.UiProxies.Add(player, proxy);
            }
            game.GlobalProxy = new ConsoleUiProxy();
            GameEngine.LoadExpansions("Expansions");
            foreach (var g in GameEngine.Expansions.Values)
            {
                game.LoadExpansion(g);
            }
            game.GameClient = client;
            game.GameServer = null;
            game.Slave = true;
            game.Run();
        }
    }
}
