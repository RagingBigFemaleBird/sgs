using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Battle.Cards;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    public class ShiYong : TriggerSkill
    {
        /// <summary>
        /// 恃勇-锁定技，每当你受到一次红色的【杀】或因【酒】生效而伤害+1的【杀】造成的伤害后，你减1点体力上限。
        /// </summary>
        public ShiYong()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    return a.ReadonlyCard != null && a.ReadonlyCard.Type is Sha &&
                        (a.ReadonlyCard.SuitColor == SuitColorType.Red || a.ReadonlyCard[Jiu.JiuSha] != 0);
                },
                (p, e, a) =>
                {
                    Game.CurrentGame.LoseMaxHealth(Owner, 1);
                },
                TriggerCondition.OwnerIsTarget
            );
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            IsEnforced = true;
        }
    }
}
