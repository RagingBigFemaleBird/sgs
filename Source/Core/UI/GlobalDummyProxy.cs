using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using System.Threading;
using System.Diagnostics;

namespace Sanguosha.Core.UI
{
    public class GlobalDummyProxy : IGlobalUiProxy
    {
        public bool AskForCardUsage(string prompt, CardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players, out Player respondingPlayer)
        {
            cards = null;
            skill = null;
            players = null;
            respondingPlayer = null;
            return false;
        }
    }
}
