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
using Sanguosha.Core.Heroes;

namespace Sanguosha.Core.Skills
{
    public abstract class PassiveSkill : ISkill
    {
        public PassiveSkill()
        {
            isAutoInvoked = false;
            Helper = new UiHelper();
        }

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

        public bool IsRulerOnly { get; protected set; }
        public bool IsSingleUse { get; protected set; }
        public bool IsAwakening { get; protected set; }
        public bool IsEnforced { get; protected set; }

        public Hero HeroTag { get; set; }

        bool? isAutoInvoked;
        public bool? IsAutoInvoked 
        {
            get
            {
                if (IsEnforced) return null;
                else return isAutoInvoked;
            }
            set
            {
                if (isAutoInvoked == value) return;
                isAutoInvoked = value;
            }
        }

        public object Clone()
        {
            var skill = Activator.CreateInstance(this.GetType()) as PassiveSkill;
            skill.Owner = this.Owner;
            skill.IsAutoInvoked = this.IsAutoInvoked;
            return skill;
        }



        public UiHelper Helper
        {
            get;
            private set;
        }
    }
}
