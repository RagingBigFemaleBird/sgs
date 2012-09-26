using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

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
                game.Players.Add(new Player());
            }
            
            game.Run();
        }
    }
}
