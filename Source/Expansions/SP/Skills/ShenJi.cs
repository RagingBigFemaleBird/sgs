using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 神戟-若你的装备区没有武器牌，当你使用【杀】时，你可以额外选择至多两个目标。
    /// </summary>
    public class ShenJi : TriggerSkill
    {

        public ShenJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p.Weapon() == null; },
                (p, e, a) =>
                {
                    ShaEventArgs args = (ShaEventArgs)a;
                    if (args.TargetApproval[0] == false)
                    {
                        return;
                    }
                    int moreTargetsToApprove = 2;
                    int i = 1;
                    while (moreTargetsToApprove > 0 && i < args.TargetApproval.Count)
                    {
                        if (args.TargetApproval[i] == true)
                        {
                            i++;
                            continue;
                        }
                        args.TargetApproval[i] = true;
                        i++;
                        moreTargetsToApprove--;
                    }
                },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(Sha.PlayerShaTargetValidation, trigger);
            IsAutoInvoked = null;
        }
    }
}
