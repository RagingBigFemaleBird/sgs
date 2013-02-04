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

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 探虎-出牌阶段，你可以与一名其他角色拼点：若你赢，你获得以下技能直到回合结束：你计算的与目标角色的距离为1，你使用的非延时类锦囊对该角色结算时不能被【无懈可击】响应。每阶段限一次。
    /// </summary>
    public class TanHu : AutoVerifiedActiveSkill
    {
        public TanHu()
        {
            MaxPlayers = 1;
            MinPlayers = 1;
            MaxCards = 0;
            LinkedPassiveSkill = new TanHuPassiveSkill();
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return player != source && player.HandCards().Count > 0;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return false;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            return source[TanHuUsed] == 0;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[TanHuUsed] = 1;
            var isWin = Game.CurrentGame.PinDian(Owner, arg.Targets[0], this);
            if (isWin == true) Owner[TanHuWin[arg.Targets[0]]] = 1;
            return true;
        }

        class TanHuPassiveSkill : TriggerSkill
        {
            public TanHuPassiveSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return p[TanHuWin[a.Targets[0]]] != 0; },
                    (p, e, a) => { var arg = a as AdjustmentEventArgs; arg.AdjustmentAmount = 1; },
                    TriggerCondition.OwnerIsSource
                    ) { AskForConfirmation = false, IsAutoNotify = false };
                Triggers.Add(GameEvent.PlayerDistanceOverride, trigger);

                var trigger2 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return p[TanHuWin[a.Targets[0]]] != 0; },
                    (p, e, a) => 
                    {
                        if (a.ReadonlyCard.Type.IsCardCategory(CardCategory.ImmediateTool))
                        {
                            a.ReadonlyCard[WuXieKeJi.CannotBeCountered[a.Targets[0]]] = 1;
                        }
                    },
                    TriggerCondition.OwnerIsSource
                    ) { AskForConfirmation = false, IsAutoNotify = false };
                Triggers.Add(GameEvent.CardUsageBeforeEffected, trigger2);
            }
        }

        private static PlayerAttribute TanHuWin = PlayerAttribute.Register("TanHuWin", true);
        private static PlayerAttribute TanHuUsed = PlayerAttribute.Register("TanHuUsed", true);
    }
}
