using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Core.Cards
{
    public abstract class CardHandler
    {
        Dictionary<DeckPlace, List<Card>> cardsOnHold;

        public abstract CardCategory Category {get;}

        /// <summary>
        /// 临时将卡牌提出，verify时使用，第二次调用将会摧毁第一次调用时临时区域的所有卡牌
        /// </summary>
        /// <param name="cards">卡牌</param>
        public virtual void HoldInTemp(List<Card> cards)
        {
            cardsOnHold = new Dictionary<DeckPlace, List<Card>>();
            foreach (Card c in cards)
            {
                if (!cardsOnHold.ContainsKey(c.Place))
                {
                    cardsOnHold.Add(c.Place, new List<Card>(Game.CurrentGame.Decks[c.Place]));
                }
            }
        }

        /// <summary>
        /// 回复临时区域的卡牌到原来位置
        /// </summary>
        public virtual void ReleaseHoldInTemp()
        {
            foreach (DeckPlace p in cardsOnHold.Keys)
            {
                Game.CurrentGame.Decks[p] = new List<Card>(cardsOnHold[p]);
            }
            cardsOnHold = null;
        }

        public virtual void Process(Player source, List<Player> dests)
        {
            foreach (var player in dests)
            {
                Process(source, player);
            }
        }

        protected abstract void Process(Player source, Player dest);

        public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> targets)
        {
            ICard card;
            if (skill != null)
            {
                CompositeCard c;
                // todo: check owner
                if (skill is CardTransformSkill)
                {
                    CardTransformSkill s = skill as CardTransformSkill;
                    VerifierResult r = s.Transform(cards, null, out c);
                    if (r != VerifierResult.Success)
                    {
                        return r;
                    }
                    if (!(this.GetType().IsAssignableFrom(c.Type.GetType())))
                    {
                        return VerifierResult.Fail;
                    }
                    HoldInTemp(c.Subcards);
                    card = c;
                }
                else
                {
                    return VerifierResult.Fail;
                }
            }
            else
            {
                if (cards == null || cards.Count != 1)
                {
                    return VerifierResult.Fail;
                }
                card = cards[0];
                if (!(this.GetType().IsAssignableFrom(card.Type.GetType())))
                {
                    return VerifierResult.Fail;
                }

                try
                {
                    Game.CurrentGame.Emit(GameEvent.PlayerCanUseCard, new Triggers.GameEventArgs()
                    {
                        Source = source,
                        Targets = targets,
                        Cards = cards
                    });
                }
                catch (TriggerResultException e)
                {
                    Trace.Assert(e.Status == TriggerResult.Fail);
                    return VerifierResult.Fail;
                }
            }

            HoldInTemp(cards);

            if (targets != null && targets.Count != 0)
            {
                try
                {
                    Game.CurrentGame.Emit(GameEvent.PlayerCanBeTargeted, new Triggers.GameEventArgs()
                    {
                        Source = Game.CurrentGame.CurrentPlayer,
                        Targets = targets,
                        Cards = cards
                    });
                }
                catch (TriggerResultException e)
                {
                    Trace.Assert(e.Status == TriggerResult.Fail);
                    ReleaseHoldInTemp();
                    return VerifierResult.Fail;
                }
            }
            VerifierResult ret = Verify(source, card, targets);
            ReleaseHoldInTemp();
            return ret;
        }

        protected abstract VerifierResult Verify(Player source, ICard card, List<Player> targets);        

        public string CardType
        {
            get { return this.GetType().Name; }
        }

    }

}
