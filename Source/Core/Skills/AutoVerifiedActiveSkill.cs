using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;

namespace Sanguosha.Core.Skills
{
    public abstract class AutoVerifiedActiveSkill : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            Trace.Assert(Owner != null);
            if (Owner == null) return VerifierResult.Fail;
            return Verify(Owner, arg.Cards, arg.Targets);
        }

        private int minPlayers;

        protected int MinPlayers
        {
            get { return minPlayers; }
            set { minPlayers = value; }
        }
        private int maxPlayers;

        protected int MaxPlayers
        {
            get { return maxPlayers; }
            set { maxPlayers = value; }
        }
        private int minCards;

        protected int MinCards
        {
            get { return minCards; }
            set { minCards = value; }
        }
        private int maxCards;

        protected int MaxCards
        {
            get { return maxCards; }
            set { maxCards = value; }
        }
        private bool discarding;
        /// <summary>
        /// Cards must pass "Game.CanDiscardCard" verification
        /// </summary>
        protected bool Discarding
        {
            get { return discarding; }
            set { discarding = value; }
        }

        public AutoVerifiedActiveSkill()
        {
            minPlayers = 0;
            maxPlayers = int.MaxValue;
            minCards = 0;
            maxCards = int.MaxValue;
            discarding = false;
        }

        protected abstract bool VerifyCard(Player source, Card card);

        protected abstract bool VerifyPlayer(Player source, Player player);

        protected virtual bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            return true;
        }

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
            bool? result = AdditionalVerify(source, cards, players);
            if (result == false)
            {
                return VerifierResult.Fail;
            }
            if (result == null)
            {
                return VerifierResult.Partial;
            }
            int count = players == null ? 0 : players.Count;
            if (count < minPlayers)
            {
                return VerifierResult.Partial;
            }
            count = cards == null ? 0 : cards.Count;
            if (count < minCards)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }
    }
}
