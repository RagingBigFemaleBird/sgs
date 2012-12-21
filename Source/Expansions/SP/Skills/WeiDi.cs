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
    public class WeiDi : PassiveSkill
    {
        public WeiDi()
        {
            IsEnforced = true;
            theSkill = new List<ISkill>();
        }
        List<ISkill> theSkill;
        protected override void InstallTriggers(Player owner)
        {
            theSkill = new List<ISkill>();
            foreach (var p in Game.CurrentGame.AlivePlayers)
            {
                if (p.Role == Role.Ruler)
                {
                    foreach (var sk in p.Hero.Skills)
                    {
                        if (sk.IsRulerOnly)
                        {
                            var toAdd = Activator.CreateInstance(sk.GetType()) as ISkill;
                            owner.AcquireAdditionalSkill(toAdd);
                            theSkill.Add(toAdd);
                        }
                    }
                }
            }
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