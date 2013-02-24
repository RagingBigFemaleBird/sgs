using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 完杀-锁定技，在你的回合，除你以外，只有处于濒死状态的角色才能使用【桃】。
    /// </summary>
    public class WanSha : TriggerSkill
    {
        public WanSha()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.CurrentPlayer == Owner && a.Source != Owner && Game.CurrentGame.DyingPlayers.Count > 0 && Game.CurrentGame.DyingPlayers.Last() != a.Source && a.Card.Type is Tao; },
                (p, e, a) => { throw new TriggerResultException(TriggerResult.Fail); },
                TriggerCondition.Global
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerCanUseCard, trigger);

            var notify = new AutoNotifyPassiveSkillTrigger(
                 this,
                 (p, e, a) => { return Game.CurrentGame.CurrentPlayer == Owner; },
                 (p, e, a) => { NotifySkillUse(a.Targets); },
                 TriggerCondition.Global
             ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerIsAboutToDie, notify);
            IsEnforced = true;
        }

    }
}
