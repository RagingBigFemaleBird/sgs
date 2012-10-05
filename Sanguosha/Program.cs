using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.UI;

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
            int id = 0;
            foreach (var g in game.CardSet)
            {
                if (g.Type is Core.Heroes.HeroCardHandler)
                {
                    Core.Heroes.HeroCardHandler h = (Core.Heroes.HeroCardHandler)g.Type;
                    Trace.TraceInformation("Assign {0} to player {1}", h.Hero.Name, id);
                    game.Players[id].Hero = h.Hero;
                    id++;
                    if (id >= 8)
                    {
                        break;
                    }
                }
            }
            game.Run();
        }
    }
}
