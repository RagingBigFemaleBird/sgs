using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Cards
{
    public class CardsAndTargetsVerifier : ICardUsageVerifier
    {
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

        public CardsAndTargetsVerifier()
        {
            minPlayers = 0;
            maxPlayers = int.MaxValue;
            minCards = 0;
            maxCards = int.MaxValue;
            discarding = false;
        }

        protected virtual bool VerifyCard(Player source, Card card)
        {
            return true;
        }

        protected virtual bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }

        protected virtual bool AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            return true;
        }

        public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            if (skill != null)
            {
                return VerifierResult.Fail;
            }
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
            if (!AdditionalVerify(source, cards, players))
            {
                return VerifierResult.Fail;
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

        public virtual IList<CardHandler> AcceptableCardTypes
        {
            get { return null; }
        }

        public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            return FastVerify(source, skill, cards, players);
        }

        public virtual UiHelper Helper
        {
            get { return new UiHelper(); }
        }
    }
}
