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
    /// 狂风-回合结束阶段开始时，你可以弃置1张“星”，指定一名角色，直到你的下回合开始，该角色每次受到的火焰伤害+1。
    /// </summary>
    public class KuangFeng : TriggerSkill
    {
        class KuangFengVerifier : CardsAndTargetsVerifier
        {
            List<Card> QiXingCards;
            public KuangFengVerifier(List<Card> qxCards)
            {
                QiXingCards = new List<Card>(qxCards);
                MaxPlayers = 1;
                MinPlayers = 1;
                MaxCards = 1;
                MinCards = 1;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return QiXingCards.Contains(card);
            }

        }

        Player kuangfengTarget;

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
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("KuangFeng"), new KuangFengVerifier(originalCards), out skill, out cards, out players))
            {
                NotifySkillUse(players);
                kuangfengTarget = players[0];
                originalCards.Remove(cards[0]);
                Game.CurrentGame.HandleCardDiscard(null, cards);
            }
            move.Cards = new List<Card>(originalCards);
            move.To = new DeckPlace(Owner, QiXing.QiXingDeck);
            Game.CurrentGame.MoveCards(move);
        }

        public KuangFeng()
        {
            kuangfengTarget = null;
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { kuangfengTarget = null; },
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
                    return (DamageElement)args.Element == DamageElement.Fire && kuangfengTarget == args.Targets[0];
                },
                (p, e, a) =>
                {
                    var args = a as DamageEventArgs;
                    args.Magnitude++;
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
