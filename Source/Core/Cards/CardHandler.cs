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

    public abstract class CardHandler : ICloneable
    {
        [NonSerialized]
        Dictionary<DeckPlace, List<Card>> deckBackup;
        [NonSerialized]
        List<Card> cardsOnHold;
        public virtual object Clone()
        {
            return Activator.CreateInstance(this.GetType());
        }

        public abstract CardCategory Category
        {
            get;
        }

        /// <summary>
        /// 临时将卡牌提出，verify时使用
        /// </summary>
        /// <param name="cards">卡牌</param>
        /// <remarks>第二次调用将会摧毁第一次调用时临时区域的所有卡牌</remarks>
        public virtual void HoldInTemp(List<Card> cards)
        {
            deckBackup = new Dictionary<DeckPlace, List<Card>>();
            foreach (Card c in cards)
            {
                if (c.Place.DeckType == DeckType.None) continue;
                Trace.Assert(c.Type != null);
                if ((c.Type is Equipment) && c.Place.DeckType == DeckType.Equipment)
                {
                    Equipment e = (Equipment)c.Type;
                    e.UnregisterTriggers(c.Place.Player);
                }
                if (!deckBackup.ContainsKey(c.Place))
                {
                    deckBackup.Add(c.Place, new List<Card>(Game.CurrentGame.Decks[c.Place]));
                }
                Game.CurrentGame.Decks[c.Place].Remove(c);
            }
            cardsOnHold = cards;
        }

        /// <summary>
        /// 回复临时区域的卡牌到原来位置
        /// </summary>
        public virtual void ReleaseHoldInTemp()
        {
            foreach (Card c in cardsOnHold)
            {
                if (c.Place.DeckType == DeckType.None) continue;
                Trace.Assert(c.Type != null);
                if ((c.Type is Equipment) && c.Place.DeckType == DeckType.Equipment)
                {
                    Equipment e = (Equipment)c.Type;
                    e.RegisterTriggers(c.Place.Player);
                }
            }
            foreach (DeckPlace p in deckBackup.Keys)
            {
                Game.CurrentGame.Decks[p].Clear();
                Game.CurrentGame.Decks[p].AddRange(deckBackup[p]);
            }
            deckBackup = null;
            cardsOnHold = null;
        }

        public void NotifyCardUse(Player source, List<Player> dests, List<Player> secondary, ICard card, GameAction action)
        {
            List<Player> logTargets = ActualTargets(source, dests, card);
            ActionLog log = new ActionLog();
            log.Source = source;
            log.Targets = logTargets;
            log.SecondaryTargets = secondary;
            log.GameAction = action;
            log.CardAction = card;
            Game.CurrentGame.NotificationProxy.NotifySkillUse(log);

            if (card is Card)
            {
                Card terminalCard = card as Card;
                if (terminalCard.Log == null) terminalCard.Log = new ActionLog();

                terminalCard.Log.Source = source;
                terminalCard.Log.Targets = dests;
                terminalCard.Log.SecondaryTargets = secondary;
                terminalCard.Log.CardAction = card;
                terminalCard.Log.GameAction = action;
            }
            else if (card is CompositeCard)
            {
                foreach (var s in (card as CompositeCard).Subcards)
                {
                    if (s.Log == null)
                    {
                        s.Log = new ActionLog();
                    }
                    s.Log.Source = source;
                    s.Log.Targets = dests;
                    s.Log.SecondaryTargets = secondary;
                    s.Log.CardAction = card;
                    s.Log.GameAction = action;
                }
            }
        }

        public virtual void TagAndNotify(Player source, List<Player> dests, ICard card, GameAction action = GameAction.Use)
        {
            NotifyCardUse(source, dests, new List<Player>(), card, action);
        }

        public virtual void Process(GameEventArgs handlerArgs)
        {
            var source = handlerArgs.Source;
            var dests = handlerArgs.Targets;
            var readonlyCard = handlerArgs.ReadonlyCard;
            var inResponseTo = handlerArgs.InResponseTo;
            var card = handlerArgs.Card;
            Game.CurrentGame.SortByOrderOfComputation(Game.CurrentGame.CurrentPlayer, dests);
            foreach (var player in dests)
            {
                if (player.IsDead) continue;
                GameEventArgs args = new GameEventArgs();
                args.Source = source;
                args.Targets = new List<Player>() { player };
                args.Card = card;
                args.ReadonlyCard = readonlyCard;
                try
                {
                    Game.CurrentGame.Emit(GameEvent.CardUsageTargetValidating, args);
                }
                catch (TriggerResultException e)
                {
                    Trace.Assert(e.Status == TriggerResult.End);
                    continue;
                }
                try
                {
                    Game.CurrentGame.Emit(GameEvent.CardUsageBeforeEffected, args);
                }
                catch (TriggerResultException e)
                {
                    Trace.Assert(e.Status == TriggerResult.End);
                    continue;
                }
                if (player.IsDead) continue;
                Process(source, player, card, readonlyCard, inResponseTo);
            }
        }

        protected abstract void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo);

        public virtual VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> targets)
        {
            return VerifyHelper(source, skill, cards, targets, IsReforging(source, skill, cards, targets));
        }

        public virtual List<Player> ActualTargets(Player source, List<Player> targets, ICard card)
        {
            return targets;
        }

        public virtual bool IsReforging(Player source, ISkill skill, List<Card> cards, List<Player> targets)
        {
            return false;
        }
        /// <summary>
        /// 卡牌UI合法性检查
        /// </summary>
        /// <param name="source"></param>
        /// <param name="skill"></param>
        /// <param name="cards"></param>
        /// <param name="targets"></param>
        /// <param name="notReforging">不是重铸中，检查PlayerCanUseCard</param>
        /// <returns></returns>
        protected VerifierResult VerifyHelper(Player source, ISkill skill, List<Card> cards, List<Player> targets, bool isReforging)
        {
            ICard card;
            if (skill != null)
            {
                CompositeCard c;
                if (skill is CardTransformSkill)
                {
                    CardTransformSkill s = skill as CardTransformSkill;
                    VerifierResult r = s.TryTransform(cards, null, out c);
                    if (c != null && c.Type != null && !(this.GetType().IsAssignableFrom(c.Type.GetType())))
                    {
                        return VerifierResult.Fail;
                    }
                    if (r != VerifierResult.Success)
                    {
                        return r;
                    }
                    if (!isReforging)
                    {
                        if (!Game.CurrentGame.PlayerCanUseCard(source, c))
                        {
                            return VerifierResult.Fail;
                        }
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

                if (!isReforging)
                {
                    if (!Game.CurrentGame.PlayerCanUseCard(source, card))
                    {
                        return VerifierResult.Fail;
                    }
                }
                HoldInTemp(cards);
            }

            var targetCheck = ActualTargets(source, targets, card);
            if (targetCheck != null && targetCheck.Count != 0)
            {
                if (!isReforging)
                {
                    if (!Game.CurrentGame.PlayerCanBeTargeted(source, targetCheck, card))
                    {
                        ReleaseHoldInTemp();
                        return VerifierResult.Fail;
                    }
                }
            }
            VerifierResult ret = Verify(source, card, targets);
            ReleaseHoldInTemp();
            return ret;
        }

        protected abstract VerifierResult Verify(Player source, ICard card, List<Player> targets);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Used by UI Only!</remarks>
        public virtual string CardType
        {
            get { return this.GetType().Name; }
        }

    }

}
