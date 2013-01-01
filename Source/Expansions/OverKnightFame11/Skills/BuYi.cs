using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 补益-当有角色进入濒死状态时，你可以展示该角色的一张手牌：若此牌不为基本牌，则该角色弃掉这张牌并回复1点体力。
    /// </summary>
    public class BuYi : TriggerSkill
    {
        protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var result = Game.CurrentGame.SelectACardFrom(eventArgs.Targets[0], Owner, new CardChoicePrompt("BuYi"), "BuYi", true);
            Game.CurrentGame.NotificationProxy.NotifyShowCard(eventArgs.Targets[0], result);
            if (result.Type.BaseCategory() != CardCategory.Basic)
            {
                Game.CurrentGame.HandleCardDiscard(eventArgs.Targets[0], new List<Card>() { result });
                Game.CurrentGame.RecoverHealth(Owner, eventArgs.Targets[0], 1);
            }
            else
            {
                Game.CurrentGame.HideHandCard(result);
            }
        }

        public BuYi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Targets[0].HandCards().Count > 0; },
                Run,
                TriggerCondition.Global
            );
            Triggers.Add(GameEvent.PlayerIsAboutToDie, trigger);
            IsAutoInvoked = null;
        }

    }
}
