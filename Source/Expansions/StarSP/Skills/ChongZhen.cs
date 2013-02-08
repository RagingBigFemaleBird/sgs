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
    /// 冲阵-每当你发动"龙胆"使用或打出一张手牌时，你可以立即获得对方的一张手牌。
    /// </summary>
    public class ChongZhen : TriggerSkill
    {
        public ChongZhen()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard[LongDan.CanShuaLiuMang] == 1 && a.Targets.Any(pl => pl.HandCards().Count > 0); },
                (p, e, a) =>
                {
                    if (a.Targets != null)
                    {
                        foreach (var target in a.Targets)
                        {
                            if (target.HandCards().Count == 0) continue;
                            var result = Game.CurrentGame.SelectACardFrom(target, p, new CardChoicePrompt("ChongZhen", target, p), "ChongZhen", true, true, true);
                            Game.CurrentGame.HandleCardTransferToHand(target, p, new List<Card>() { result });
                        }
                    }
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PlayerPlayedCard, trigger);
            Triggers.Add(GameEvent.PlayerUsedCard, trigger);
            IsAutoInvoked = null;
        }
    }
}
