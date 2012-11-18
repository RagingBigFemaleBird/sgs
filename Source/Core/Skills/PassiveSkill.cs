using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Core.Players;

namespace Sanguosha.Core.Skills
{
    public abstract class PassiveSkill : ISkill
    {
        private Players.Player owner;
        
        /// <summary>
        /// Owner of the skill.
        /// </summary>
        /// <remarks>If you override this, you should install triggers when owner changes.</remarks>
        public virtual Players.Player Owner
        {
            get { return owner; }
            set 
            {
                if (owner == value) return;
                if (owner != null)
                {
                    UninstallTriggers(owner);
                }
                owner = value;
                if (owner != null)
                {
                    InstallTriggers(owner);
                }
            }
        }

        protected abstract void InstallTriggers(Players.Player owner);

        protected abstract void UninstallTriggers(Players.Player owner);

        /// <summary>
        /// 返回该技能是否是主公技。
        /// </summary>
        public virtual bool IsRulerOnly { get { return false; } }

        /// <summary>
        /// 返回该技能是否是限定技。
        /// </summary>
        public virtual bool IsSingleUse { get { return false; } }

        /// <summary>
        /// 返回该技能是否是觉醒技。
        /// </summary>
        public virtual bool IsAwakening { get { return false; } }

        /// <summary>
        /// 返回该技能是否是锁定技。
        /// </summary>
        public virtual bool IsEnforced { get { return false; } }
    }
}
