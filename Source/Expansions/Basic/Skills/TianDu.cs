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
    /// 天妒-在你的判定牌生效后，你可以获得此牌。
    /// </summary>
    public class TianDu : TriggerSkill
    {
        delegate bool _AskTianDuDelegate();
        class AlwaysGetJudgeCardTrigger : GetJudgeCardTrigger
        {
            protected override bool IsCorrectJudgeAction(ISkill skill, ICard card)
            {
                if (askDel())
                {
                    this.skill.NotifySkillUse();
                    return true;
                }
                return false;
            }
            _AskTianDuDelegate askDel;
            TriggerSkill skill;
            public AlwaysGetJudgeCardTrigger(Player owner, _AskTianDuDelegate del, TriggerSkill skill) : base(owner, null, null, true) 
            { 
                askDel = del;
                this.skill = skill;
            }
        }

        public TianDu()
        {
            Triggers.Add(GameEvent.PlayerJudgeDone, new AlwaysGetJudgeCardTrigger(Owner, AskForSkillUse, this) { Priority = int.MinValue });
            IsAutoInvoked = true;
        }
    }
}
