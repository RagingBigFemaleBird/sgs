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
    /// 马术-锁定技，当你计算与其他角色的距离时，始终-1。
    /// </summary>
    public class MaShu : PassiveSkill
    {
        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            owner[Player.RangeMinus]--;
        }

        protected override void UninstallTriggers(Player owner)
        {
            owner[Player.RangeMinus]++;
        }

        public override bool isEnforced
        {
            get
            {
                return true;
            }
        }
    }
}
