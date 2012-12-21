using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;


namespace Sanguosha.Core.Games
{
    public abstract partial class Game
    {
        void _FilterCard(Player p, Card card)
        {
            GameEventArgs args = new GameEventArgs();
            args.Source = p;
            args.Card = card;
            Emit(GameEvent.EnforcedCardTransform, args);
        }

        void _ResetCard(Card card)
        {
            if (card.Id > 0)
            {
                card.Type = GameEngine.CardSet[card.Id].Type;
                card.Suit = GameEngine.CardSet[card.Id].Suit;
                card.Rank = GameEngine.CardSet[card.Id].Rank;
            }
        }

        void _ResetCards(Player p)
        {
            foreach (var card in decks[p, DeckType.Hand].Concat(decks[p, DeckType.Equipment]).Concat(decks[p, DeckType.DelayedTools]))
            {
                if (card.Id > 0)
                {
                    _ResetCard(card);
                    _FilterCard(p, card);
                }
            }
        }

        public void PlayerAcquireSkill(Player p, ISkill skill)
        {
            p.AcquireAdditionalSkill(skill);
            _ResetCards(p);
        }

        public void PlayerLostSkill(Player p, ISkill skill)
        {
            p.LoseAdditionalSkill(skill);
            _ResetCards(p);
        }

        public int NumberOfAliveAllegiances
        {
            get
            {
                var ret =
                (from p in Game.CurrentGame.AlivePlayers select p.Allegiance).Distinct().Count();
                return ret;
            }
        }
    }
}
