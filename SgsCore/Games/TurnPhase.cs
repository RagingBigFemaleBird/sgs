using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Games
{
    public enum TurnPhase
    {
        BeforeTurnStart,
        TurnStart,
        Judgement,
        Dealing,
        Playing,
        Discarding,
        TurnFinish,
        AfterTurnFinish
    }
}
