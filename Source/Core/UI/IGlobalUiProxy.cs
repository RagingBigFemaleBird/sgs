using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.UI
{
    public interface IGlobalUiProxy
    {
        bool AskForCardUsage(Prompt prompt, CardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players, out Player respondingPlayer);
    }
}
