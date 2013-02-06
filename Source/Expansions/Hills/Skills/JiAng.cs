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

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 激昂-每当你使用(指定目标后)或被使用(成为目标后)一张【决斗】或红色的【杀】时，你可以摸一张牌。
    /// </summary>
    public class JiAng : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            if (Owner.Hero.Name == "SPLvMeng" || (Owner.Hero2 != null && Owner.Hero2.Name == "SPLvMeng")) return 1;
            return 0;
        }

        public JiAng()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return (a.Source == p || a.Targets.Contains(p)) && ((a.ReadonlyCard.Type is JueDou) || ((a.ReadonlyCard.Type is Sha) && a.ReadonlyCard.SuitColor == SuitColorType.Red));},
                (p, e, a) => { Game.CurrentGame.DrawCards(p, 1); },
                TriggerCondition.Global
            );
            Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger);
            IsAutoInvoked = true;
        }
    }
}
