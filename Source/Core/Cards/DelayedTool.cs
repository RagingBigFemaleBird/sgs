using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Core.Cards
{
    
    public abstract class DelayedTool : CardHandler
    {
        public override CardCategory Category
        {
            get { return CardCategory.DelayedTool; }
        }

        protected void AttachTo(Player source, Player target, ICard c)
        {
            CardsMovement m;
            if (c is CompositeCard)
            {
                m.cards = new List<Card>(((CompositeCard)c).Subcards);
            }
            else
            {
                m.cards = new List<Card>();
                Card card = (Card)c;
                Trace.Assert(card != null);
                m.cards.Add(card);
            }
            m.to = new DeckPlace(target, DeckType.DelayedTools);
            Game.CurrentGame.MoveCards(m, new CardUseLog() { Source = source, Target = target, Cards = null, Skill = null });
        }

        protected bool DelayedToolConflicting(Player p)
        {
            foreach (Card c in Game.CurrentGame.Decks[p, DeckType.DelayedTools])
            {
                if (this.GetType().IsAssignableFrom(c.Type.GetType()))
                {
                    return true;
                }
            }
            return false;
        }

        public abstract void Activate(Player p, Card c);

    }
}
