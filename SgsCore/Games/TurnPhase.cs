using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Games
{
    public enum TurnPhase
    {
        BeforeTurnStart = 0,
        TurnStart = 1,
        Judgement = 2,
        Dealing = 3,
        Playing = 4,
        Discarding = 5,
        TurnFinish = 6,
        AfterTurnFinish = 7
    }
}
