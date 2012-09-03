using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Games;

namespace Sanguosha
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new RoleGame();
            game.Run();
        }
    }
}
