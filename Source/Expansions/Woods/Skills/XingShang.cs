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

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 行殇-你可以获得死亡角色的所有牌。
    /// </summary>
    public class XingShang : TriggerSkill
    {
        public XingShang()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Targets[0] != p; },
                (p, e, a) =>
                {
                    List<Card> toGet = new List<Card>();
                    toGet.AddRange(a.Targets[0].Equipments());
                    toGet.AddRange(a.Targets[0].HandCards());
                    Game.CurrentGame.HandleCardTransferToHand(null, p, toGet);
                },
                TriggerCondition.Global
            );

            Triggers.Add(GameEvent.PlayerIsDead, trigger);
            IsAutoInvoked = true;
        }
    }
}
