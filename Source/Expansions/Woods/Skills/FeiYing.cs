using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 飞影-锁定技，当其他角色计算与你的距离时，始终+1。
    /// </summary>
    public class FeiYing : PassiveSkill
    {
        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            owner[Player.RangePlus]++;
        }

        protected override void UninstallTriggers(Player owner)
        {
            owner[Player.RangePlus]--;
        }
        public FeiYing()
        {
            IsEnforced = true;
        }
    }
}