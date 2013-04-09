using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    /// <summary>
    /// 精策-出牌阶段结束时，若本回合使用的牌数量大于你当前体力值，你可以回复1点体力或摸一张牌。
    /// </summary>
    public class JingCe : TriggerSkill
    {
        int _count;
        public JingCe()
        {
            _count = 0;
            var cardUsedCount = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    _count++;
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerUsedCard, cardUsedCount);

            var tagClear = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    _count = 0;
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeforeStart, tagClear);

            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return _count > p.Health; },
                (p, e, a) =>
                {
                    int answer = 0;
                    List<OptionPrompt> prompts = new List<OptionPrompt>();
                    prompts.Add(OptionPrompt.NoChoice);
                    prompts.Add(new OptionPrompt("JingCeMoPai"));
                    if (p.LostHealth > 0)
                    {
                        prompts.Add(new OptionPrompt("JingCeHuiXue"));
                    }
                    p.AskForMultipleChoice(new MultipleChoicePrompt("JingCe"), prompts, out answer);
                    if (answer == 1)
                    {
                        NotifySkillUse();
                        Game.CurrentGame.DrawCards(p, 1);
                    }
                    else if (answer == 2)
                    {
                        NotifySkillUse();
                        Game.CurrentGame.RecoverHealth(p, p, 1);
                    }
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseEndEvents[TurnPhase.Play], trigger);

            IsAutoInvoked = null;
        }
    }
}
