using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Cards
{
    public class OneTargetNoSelfVerifier : ICardUsageVerifier
    {

        public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            if (skill != null || (cards != null && cards.Count != 0))
            {
                return VerifierResult.Fail;
            }
            if (players == null || players.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (players.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (players[0] == source)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public IList<CardHandler> AcceptableCardTypes
        {
            get { return null; }
        }

        public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            return FastVerify(source, skill, cards, players);
        }

        public UiHelper Helper
        {
            get { return new UiHelper(); }
        }
    }
}
