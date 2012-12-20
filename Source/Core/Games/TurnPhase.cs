using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Games
{
    public enum TurnPhase
    {
        Inactive = -1,
        BeforeStart = 0,
        Start = 1,
        Judge = 2,
        Draw = 3,
        Play = 4,
        Discard = 5,
        End = 6,
        PostEnd = 7,
    }
}
