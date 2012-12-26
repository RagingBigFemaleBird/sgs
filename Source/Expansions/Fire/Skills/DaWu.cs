using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 大雾-回合结束阶段开始时，你可以弃掉X张“星”，指定X名角色，直到你的下回合开始，防止他们受到的除雷电伤害外的所有伤害。
    /// </summary>
    public class DaWu : TriggerSkill
    {
        class DaWuVerifier : CardsAndTargetsVerifier
        {
            List<Card> QiXingCards;
            public DaWuVerifier(List<Card> qxCards)
            {
                QiXingCards = new List<Card>(qxCards);
                MaxPlayers = qxCards.Count;
                MinPlayers = 1;
                MaxCards = qxCards.Count;
                MinCards = 1;
            }

            protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
            {
                int cp, cc;
                if (players == null) cp = 0; else cp = players.Count;
                if (cards == null) cc = 0; else cc = cards.Count;
                if (cp != cc) return null;
                return true;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return QiXingCards.Contains(card);
            }

        }

        List<Player> dawuTargets;
        public static readonly PlayerAttribute DaWuMark = PlayerAttribute.Register("DaWu", false, true);

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            List<Card> originalCards = new List<Card>(Game.CurrentGame.Decks[Owner, QiXing.QiXingDeck]);
            int qxCount = Game.CurrentGame.Decks[Owner, QiXing.QiXingDeck].Count;
            // hack the cards to owner's hand. do not trigger anything
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(Game.CurrentGame.Decks[Owner, QiXing.QiXingDeck]);
            move.To = new DeckPlace(Owner, DeckType.Hand);
            move.Helper.IsFakedMove = true;
            Game.CurrentGame.MoveCards(move);
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("DaWu"), new DaWuVerifier(originalCards), out skill, out cards, out players))
            {
                NotifySkillUse(players);
                foreach (var mark in players)
                {
                    mark[DaWuMark] = 1;
                }
                dawuTargets = players;
                foreach (Card cc in cards) originalCards.Remove(cc);
                Game.CurrentGame.HandleCardDiscard(null, cards);
            }
            move.Cards = new List<Card>(originalCards);
            move.To = new DeckPlace(Owner, QiXing.QiXingDeck);
            Game.CurrentGame.MoveCards(move);
        }

        public DaWu()
        {
            dawuTargets = new List<Player>();
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { foreach (var mark in dawuTargets) { mark[DaWuMark] = 0; } dawuTargets.Clear(); },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[Owner, QiXing.QiXingDeck].Count > 0; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            var trigger3 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    var args = a as DamageEventArgs;
                    return (DamageElement)args.Element != DamageElement.Lightning && dawuTargets.Contains(args.Targets[0]);
                },
                (p, e, a) =>
                {
                    throw new TriggerResultException(TriggerResult.End);
                },
                TriggerCondition.Global
            ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger2);
            Triggers.Add(GameEvent.DamageInflicted, trigger3);
            IsAutoInvoked = false;
        }

    }
}
