using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Exceptions
{
    
    public class PlayerIsDeadException : SgsException
    {
        public Player Player { get; set; }
        public PlayerIsDeadException(Player p)
        {
            Player = p;
        }
    }
}
