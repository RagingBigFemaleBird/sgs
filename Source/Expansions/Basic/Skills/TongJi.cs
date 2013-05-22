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

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 同疾-锁定技，若你的手牌数大于你的体力值，则你只要在任一其他角色的攻击范围内，该角色使用杀时便不能指定你以外的角色为目标。
    /// </summary>
    public class TongJi : TriggerSkill
    {
        public TongJi()
        {
            Triggers.Add(GameEvent.PlayerCanBeTargeted, new RelayTrigger(
                (p, e, a) =>
                {
                    return a.Source != p && (a.Card.Type is Sha) && p.HandCards().Count > p.Health && !a.Targets.Contains(p) && Game.CurrentGame.DistanceTo(a.Source, p) <= a.Source[Player.AttackRange] + 1;
                },
                (p, e, a) =>
                {
                    throw new TriggerResultException(TriggerResult.Fail);
                },
                TriggerCondition.Global
                ));
            IsEnforced = true;

            var notifier2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source != p && (a.Card.Type is Sha) && p.HandCards().Count > p.Health && a.Targets.Contains(p) && Game.CurrentGame.DistanceTo(a.Source, p) <= a.Source[Player.AttackRange] + 1; },
                (p, e, a) => { },
                TriggerCondition.Global
            );

            Triggers.Add(GameEvent.CardUsageTargetConfirmed, notifier2);
        }

    }
}
