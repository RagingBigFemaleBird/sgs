using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Games
{
    public enum TurnPhase
    {
        Inactive = -1,
        Start = 0,
        Judge = 1,
        Draw = 2,
        Play = 3,
        Discard = 4,
        End = 5,
    }
}
