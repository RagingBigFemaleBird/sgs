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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 陷阵-出牌阶段，你可以与一名其他角色拼点。若你赢，你获得以下技能直到回合结束：你无视与该角色的距离及其防具；你对该角色使用【杀】时无次数限制。若你没赢，你不能使用【杀】，直到回合结束。每阶段限一次。
    /// </summary>
    public class XianZhen : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[XianZhenUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if ((arg.Targets != null && arg.Targets.Count > 1) || arg.Targets[0].HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets[0] == Owner || Owner.HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[XianZhenUsed] = 1;
            var result = Game.CurrentGame.PinDian(Owner, arg.Targets[0], this);
            if (!result)
            {
                var trigger = new XianZhenPassiveLose(Owner);
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanUseCard, trigger);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new XianZhenLoseRemoval(Owner, trigger));
            }
            else
            {
                arg.Targets[0][Armor.PlayerIgnoreArmor[Owner]] = 1;
                var trigger = new XianZhenPassiveWin1(Owner, arg.Targets[0]);
                Game.CurrentGame.RegisterTrigger(Sha.PlayerShaTargetValidation, trigger);
                var trigger2 = new XianZhenPassiveWin2(Owner);
                Game.CurrentGame.RegisterTrigger(Sha.PlayerNumberOfShaCheck, trigger2);
                var trigger3 = new XianZhenPassiveWin3(Owner, arg.Targets[0]);
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerDistanceOverride, trigger3);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new XianZhenWinRemoval(Owner, arg.Targets[0], trigger, trigger2, trigger3));
            }
            return true;
        }

        public XianZhen()
        {
        }

        private static PlayerAttribute XianZhenUsed = PlayerAttribute.Register("XianZhenUsed", true);

        public class XianZhenPassiveLose : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source == Owner && eventArgs.Card.Type is Sha)
                {
                    throw new TriggerResultException(TriggerResult.Fail);
                }
            }
            public XianZhenPassiveLose(Player p)
            {
                Owner = p;
            }
        }

        public class XianZhenPassiveWin1 : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ShaEventArgs args = (ShaEventArgs)eventArgs;
                Trace.Assert(args != null);
                if (args.Source != Owner) return;
                if (args.Targets[0] == target)
                {
                    args.TargetApproval[0] = true;
                }
            }
            Player target;
            public XianZhenPassiveWin1(Player p, Player target)
            {
                Owner = p;
                this.target = target;
            }
        }

        public class XianZhenPassiveWin2 : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner) return;
                throw new TriggerResultException(TriggerResult.Success);
            }
            public XianZhenPassiveWin2(Player p)
            {
                Owner = p;
            }
        }

        public class XianZhenPassiveWin3 : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                var arg = eventArgs as AdjustmentEventArgs;
                if (arg.Source == Owner && arg.Targets[0] == target) arg.AdjustmentAmount = 1;
            }
            Player target;
            public XianZhenPassiveWin3(Player p, Player target)
            {
                Owner = p;
                this.target = target;
            }
        }

        public class XianZhenLoseRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                    return;
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanUseCard, trigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }

            private Trigger trigger;
            public XianZhenLoseRemoval(Player p, Trigger trigger)
            {
                Owner = p;
                this.trigger = trigger;
            }
        }

        public class XianZhenWinRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                    return;
                target[Armor.PlayerIgnoreArmor[Owner]] = 0;
                Game.CurrentGame.UnregisterTrigger(Sha.PlayerShaTargetValidation, trigger);
                Game.CurrentGame.UnregisterTrigger(Sha.PlayerNumberOfShaCheck, trigger2);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerDistanceOverride, trigger3);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }

            private Trigger trigger, trigger2, trigger3;
            Player target;
            public XianZhenWinRemoval(Player p, Player target, Trigger trigger, Trigger trigger2, Trigger trigger3)
            {
                Owner = p;
                this.trigger = trigger;
                this.trigger2 = trigger2;
                this.trigger3 = trigger3;
                this.target = target;
            }
        }

    }
}
