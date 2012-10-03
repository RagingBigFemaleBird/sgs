using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;

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
                owner = value;
                InstallTriggers(owner);
            }
        }

        protected abstract void InstallTriggers(Players.Player owner);
    }
}
