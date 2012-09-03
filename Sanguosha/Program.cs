using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

namespace Sanguosha
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new RoleGame();
            for (int i = 0; i < 8; i++)
            {
                game.Players.Add(new Player());
            }
            game.Run();
        }
    }
}
