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

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 伪帝-锁定技，你视为拥有当前主公的主公技。
    /// </summary>
    public class WeiDi : TriggerSkill
    {
        public WeiDi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source == ruler; },
                (p, e, a) => { UninstallSkills(); InstallSkills(p);},
                TriggerCondition.Global
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerSkillSetChanged, trigger);

            IsEnforced = true;
            ruler = null;
            theSkill = new List<ISkill>();
        }
        List<ISkill> theSkill;
        Player ruler;
        protected override void InstallTriggers(Player owner)
        {
            InstallSkills(owner);
            base.InstallTriggers(owner);
        }

        void InstallSkills(Player owner)
        {
            theSkill = new List<ISkill>();
            foreach (var p in Game.CurrentGame.AlivePlayers)
            {
                if (p.Role == Role.Ruler)
                {
                    ruler = p;
                    foreach (var sk in p.ActionableSkills)
                    {
                        if (sk.IsRulerOnly)
                        {
                            var toAdd = Activator.CreateInstance(sk.GetType()) as ISkill;
                            Game.CurrentGame.PlayerAcquireSkill(owner, toAdd);
                            theSkill.Add(toAdd);
                        }
                    }
                }
            }
        }

        void UninstallSkills()
        {
            foreach (var skill in theSkill)
            {
                Game.CurrentGame.PlayerLostSkill(Owner, skill);
            }
            theSkill.Clear();
        }

        protected override void UninstallTriggers(Player owner)
        {
            foreach (var sk in theSkill)
            {
                owner.LoseAdditionalSkill(sk);
            }
        }
    }
}