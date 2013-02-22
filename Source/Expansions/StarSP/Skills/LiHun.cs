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
using Sanguosha.Expansions.Basic.Skills;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 离魂―出牌阶段，你可以弃置一张牌并将你的武将牌翻面，若如此做，选择一名男性角色，获得其所有手牌；出牌阶段结束时，你须交给该角色等同于其当前体力值数的牌。每阶段限一次。
    /// </summary>
    public class LiHun : ActiveSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets, List<Card> cards)
        {
            return Game.CurrentGame.CurrentPhaseEventIndex - 1;
        }

        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[LiHunUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            if (Owner.HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && (!arg.Targets[0].IsMale || arg.Targets[0].HandCards().Count == 0))
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (cards != null && cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        class LiHunVerifier : CardsAndTargetsVerifier
        {
            public LiHunVerifier(int count)
            {
                MaxCards = count;
                MinCards = count;
                MaxPlayers = 0;
                MinPlayers = 0;
                Discarding = false;
                Helper.NoCardReveal = true;
            }
        }

        class LiHunGiveBack : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                while (true)
                {
                    if (Owner.HandCards().Count + Owner.Equipments().Count == 0 || target.Health <= 0)
                        break;
                    int toGiveBack = target.Health;
                    List<Card> cards = new List<Card>();
                    if (toGiveBack >= Owner.HandCards().Count + Owner.Equipments().Count)
                    {
                        cards.AddRange(Owner.HandCards());
                        cards.AddRange(Owner.Equipments());
                    }
                    else
                    {
                        ISkill skill;
                        List<Player> players;
                        if (!Owner.AskForCardUsage(new CardUsagePrompt("LiHun", target, target.Health), new LiHunVerifier(toGiveBack), out skill, out cards, out players))
                        {
                            cards.AddRange(Owner.HandCards());
                            cards.AddRange(Owner.Equipments());
                            cards = cards.GetRange(0, target.Health);
                        }
                    }

                    lihun.NotifyAction(Owner, new List<Player>(), new List<Card>());
                    Game.CurrentGame.NotificationProxy.NotifyLogEvent(new LogEvent("LiHun", lihun, Owner, target, cards.Count), new List<Player>() { Owner, target });
                    Game.CurrentGame.HandleCardTransferToHand(Owner, target, cards);
                    break;
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.Play], this);
            }
            Player target;
            LiHun lihun;
            public LiHunGiveBack(Player diaochan, Player target, LiHun skill)
            {
                Owner = diaochan;
                this.target = target;
                lihun = skill;
            }
        }

        public override void NotifyAction(Player source, List<Player> targets, List<Card> cards)
        {
            ActionLog log = new ActionLog();
            log.GameAction = GameAction.None;
            log.CardAction = null;
            log.SkillAction = this;
            log.Source = source;
            log.Targets = targets;
            foreach (Card c in cards)
            {
                if (c.Log == null)
                {
                    c.Log = new ActionLog();
                }
                c.Log.SkillAction = this;
            }
            int index = GenerateSpecialEffectHintIndex(source, targets, cards);
            log.SpecialEffectHint = index;
            log.SkillSoundOnly = index == 1 ? true : false;
            Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[LiHunUsed] = 1;
            Game.CurrentGame.HandleCardDiscard(Owner, arg.Cards);
            Owner.IsImprisoned = !Owner.IsImprisoned;
            Game.CurrentGame.HandleCardTransferToHand(arg.Targets[0], Owner, arg.Targets[0].HandCards());
            Game.CurrentGame.RegisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.Play], new LiHunGiveBack(Owner, arg.Targets[0], this));
            return true;
        }

        private static PlayerAttribute LiHunUsed = PlayerAttribute.Register("LiHunUsed", true);
    }
}
