using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 无谋―锁定技，当你使用一张非延时类锦囊牌选择目标后，你须弃1枚“暴怒”标记或失去1点体力。
    /// </summary>
    public class WuMou : TriggerSkill
    {
        public WuMou()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return CardCategoryManager.IsCardCategory(a.Card.Type.Category, CardCategory.ImmediateTool); },
                (p, e, a) =>
                {
                    if (Owner[KuangBao.BaoNuMark] == 0)
                    {
                        Game.CurrentGame.LoseHealth(Owner, 1);
                    }
                    else
                    {
                        int answer = 0;
                        Owner.AskForMultipleChoice(
                            new MultipleChoicePrompt("WuMou"),
                            new List<OptionPrompt>() { new OptionPrompt("WuMouMark"), new OptionPrompt("WuMouHealth") },
                            out answer);
                        if (answer == 0)
                        {
                            Owner[KuangBao.BaoNuMark]--;
                        }
                        else
                        {
                            Game.CurrentGame.LoseHealth(Owner, 1);
                        }
                    }
                },
                TriggerCondition.OwnerIsSource
            ) { Type = TriggerType.Skill };
            Triggers.Add(GameEvent.PlayerUsedCard, trigger);
            IsEnforced = true;
        }
    }
}
