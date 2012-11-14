using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.Skills
{
    public abstract class AutoVerifiedActiveSkill : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            return Verify(Owner, arg.Cards, arg.Targets);
        }

        protected int minPlayers;
        protected int maxPlayers;
        protected int minCards;
        protected int maxCards;
        protected bool discarding;

        public AutoVerifiedActiveSkill()
        {
            minPlayers = 0;
            maxPlayers = 0;
            minCards = 0;
            maxCards = 0;
            discarding = false;
        }

        protected abstract bool VerifyCard(Player source, Card card);

        protected abstract bool VerifyPlayer(Player source, Player player);

        public VerifierResult Verify(Player source, List<Card> cards, List<Player> players)
        {
            if (cards != null && cards.Count > maxCards)
            {
                return VerifierResult.Fail;
            }
            if (cards != null && cards.Count > 0)
            {
                foreach (Card c in cards)
                {
                    if (discarding && !Game.CurrentGame.PlayerCanDiscardCard(source, c))
                    {
                        return VerifierResult.Fail;
                    }
                    if (!VerifyCard(source, c))
                    {
                        return VerifierResult.Fail;
                    }
                }
            }
            if (players != null && players.Count > maxPlayers)
            {
                return VerifierResult.Fail;
            }
            if (players != null && players.Count > 0)
            {
                foreach (Player p in players)
                {
                    if (!VerifyPlayer(source, p))
                    {
                        return VerifierResult.Fail;
                    }
                }
            }
            int count = players == null ? 0 : players.Count;
            if (count < minPlayers)
            {
                return VerifierResult.Partial;
            }
            count = cards == null ? 0 : cards.Count;
            if (cards == null || cards.Count < minCards)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }
    }
}
