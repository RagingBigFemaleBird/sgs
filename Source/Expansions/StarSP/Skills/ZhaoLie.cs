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
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 昭烈–摸牌阶段摸牌时，你可以少摸一张牌并选择你攻击范围内的一名其他角色，然后亮出牌堆顶的三张牌，将其中的非基本牌和【桃】置入弃牌堆，该角色须选择一项：1、你对其造成X点伤害，然后他获得这些基本牌；1、他依次弃置X张牌，然后你获得这些基本牌。（X为其中非基本牌的数量）
    /// </summary>
    public class ZhaoLie : TriggerSkill
    {
        class ZhaoLieVerifier : CardsAndTargetsVerifier
        {
            public ZhaoLieVerifier()
            {
                MaxCards = 0;
                MinCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
                Discarding = false;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return source != player && Game.CurrentGame.DistanceTo(source, player) <= source[Player.AttackRange] + 1;
            }
        }

        void ZhaoLieProcess(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ZhaoLieUsed = false;
            basicCards.Clear();
            notBasicCards.Clear();
            List<Card> tao = new List<Card>();
            DeckType ZhaoLieDeck = new DeckType("ZhaoLie");
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>();
            int toDraw = 3;
            for (int i = 0; i < toDraw; i++)
            {
                Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                c.Log.SkillAction = this;
                c.Log.Source = Owner;
                move.Cards.Add(c);
                Game.CurrentGame.NotificationProxy.NotifyShowCard(null, c);
                if (c.Type is Tao) tao.Add(c);
                else if (c.Type.IsCardCategory(CardCategory.Basic)) basicCards.Add(c);
                else notBasicCards.Add(c);
            }
            move.To = new DeckPlace(null, ZhaoLieDeck);
            Game.CurrentGame.MoveCards(move);
            int answer = 0;
            List<OptionPrompt> prompts = new List<OptionPrompt>();
            prompts.Add(new OptionPrompt("ZhaoLieShangHai", notBasicCards.Count, basicCards.Count));
            prompts.Add(new OptionPrompt("ZhaoLieQiPai", notBasicCards.Count, basicCards.Count));
            while (true)
            {
                if (ZhaoLieTarget.HandCards().Count + ZhaoLieTarget.Equipments().Count < notBasicCards.Count) break;
                ZhaoLieTarget.AskForMultipleChoice(new MultipleChoicePrompt("ZhaoLie"), prompts, out answer);
                break;
            }
            List<Card> toDiscard = new List<Card>();
            toDiscard.AddRange(notBasicCards);
            toDiscard.AddRange(tao);
            foreach (Card c in toDiscard)
            {
                c.Log = new ActionLog();
                c.Log.SkillAction = this;
                c.Log.GameAction = GameAction.PlaceIntoDiscard;
            }
            Game.CurrentGame.PlaceIntoDiscard(null, toDiscard);
            if (answer == 0)
            {
                if (notBasicCards.Count == 0) Game.CurrentGame.HandleCardTransferToHand(null, ZhaoLieTarget, basicCards);
                else
                {
                    ReadOnlyCard rCard = new ReadOnlyCard(new Card() { Place = new DeckPlace(null, null) });
                    rCard[ZhaoLieDamage] = 1;
                    Game.CurrentGame.DoDamage(Owner, ZhaoLieTarget, notBasicCards.Count, DamageElement.None, null, rCard);
                }
            }
            else
            {
                for (int i = 0; i < notBasicCards.Count; i++)
                {
                    Game.CurrentGame.ForcePlayerDiscard(ZhaoLieTarget, (pl, d) => { return 1 - d; }, true);
                }
                Game.CurrentGame.HandleCardTransferToHand(null, Owner, basicCards);
            }
            ZhaoLieTarget = null;
        }

        List<Card> basicCards = new List<Card>();
        List<Card> notBasicCards = new List<Card>();
        Player ZhaoLieTarget = null;
        bool ZhaoLieUsed = false;
        public ZhaoLie()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                   this,
                   (p, e, a) =>
                   {
                       return Game.CurrentGame.AlivePlayers.Any(pl => Game.CurrentGame.DistanceTo(p, pl) <= p[Player.AttackRange] + 1 && pl != p);
                   },
                   (p, e, a, c, pls) =>
                   {
                       p[Player.DealAdjustment]--;
                       ZhaoLieTarget = pls[0];
                       ZhaoLieUsed = true;
                   },
                   TriggerCondition.OwnerIsSource,
                   new ZhaoLieVerifier()
               ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                   this,
                   (p, e, a) => { return ZhaoLieUsed && ZhaoLieTarget != null; },
                   ZhaoLieProcess,
                   TriggerCondition.OwnerIsSource
               ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseEndEvents[TurnPhase.Draw], trigger2);

            var trigger3 = new AutoNotifyPassiveSkillTrigger(
                   this,
                   (p, e, a) => { return ZhaoLieTarget != null && a.Targets.Contains(ZhaoLieTarget) && a.ReadonlyCard != null && a.ReadonlyCard[ZhaoLieDamage] != 0; },
                   (p, e, a) => { a.ReadonlyCard[ZhaoLieDamage[ZhaoLieTarget]] = 1; },
                   TriggerCondition.OwnerIsSource
               ) { AskForConfirmation = false, IsAutoNotify = false, Priority = int.MinValue };
            Triggers.Add(GameEvent.DamageInflicted, trigger3);

            var trigger4 = new AutoNotifyPassiveSkillTrigger(
                   this,
                   (p, e, a) => { return ZhaoLieTarget != null && a.ReadonlyCard != null && a.ReadonlyCard[ZhaoLieDamage[a.Targets[0]]] != 0; },
                   (p, e, a) => { Game.CurrentGame.HandleCardTransferToHand(null, a.Targets[0], basicCards); ZhaoLieTarget = null; },
                   TriggerCondition.OwnerIsSource
               ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.DamageComputingFinished, trigger4);

            IsAutoInvoked = null;
        }

        private static CardAttribute ZhaoLieDamage = CardAttribute.Register("ZhaoLieDamage");
    }
}
