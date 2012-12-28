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
        Dictionary<ISkill, ISkill> theSkills;
        Player ruler;
        public WeiDi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source == ruler; },
                (p, e, a) => 
                {
                    bool nothingChanged = true;
                    foreach (var sk in a.Source.ActionableSkills)
                    {
                        if (sk.IsRulerOnly)
                        {
                            if (!theSkills.ContainsKey(sk))
                                nothingChanged = false;
                        }
                    }
                    if (!nothingChanged)
                    {
                        UninstallSkills();
                        InstallSkills(p);
                    }
                },
                TriggerCondition.Global
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerSkillSetChanged, trigger);

            IsEnforced = true;
            ruler = null;
            theSkills = new Dictionary<ISkill, ISkill>();
        }
        protected override void InstallTriggers(Player owner)
        {
            InstallSkills(owner);
            base.InstallTriggers(owner);
        }

        void InstallSkills(Player owner)
        {
            theSkills = new Dictionary<ISkill, ISkill>();
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
                            theSkills.Add(sk, toAdd);
                        }
                    }
                }
            }
        }

        void UninstallSkills()
        {
            foreach (var skill in theSkills)
            {
                Game.CurrentGame.PlayerLostSkill(Owner, skill.Value);
            }
            theSkills.Clear();
        }

        protected override void UninstallTriggers(Player owner)
        {
            UninstallSkills();
        }
    }
}