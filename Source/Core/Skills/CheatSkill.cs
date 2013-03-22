using Sanguosha.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.UI;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Core.Skills
{
    public enum CheatType
    {
        Card,
        Skill,
    }

    public class CheatSkill : ISkill
    {
        public CheatSkill()
        {
            Helper = new UiHelper();
            HeroTag = null;
        }
        public CheatType CheatType { get; set; }
        public int CardId { get; set; }

        /// <summary>
        /// Sets/gets name of the skill to be acquired by the CheatSkill.
        /// </summary>
        public string SkillName { get; set; }

        private Players.Player owner;

        public Players.Player Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public bool IsRulerOnly
        {
            get { return false; }
        }

        public bool IsSingleUse
        {
            get { return false; }
        }

        public bool IsAwakening
        {
            get { return false; }
        }

        public bool IsEnforced
        {
            get { return false; }
        }

        public object Clone()
        {
            var skill = Activator.CreateInstance(this.GetType()) as CheatSkill;
            skill.Owner = this.Owner;
            skill.CheatType = this.CheatType;
            skill.CardId = this.CardId;
            skill.SkillName = this.SkillName;
            return skill;
        }

        public Hero HeroTag
        {
            get;
            set;
        }

        public UiHelper Helper
        {
            get;
            private set;
        }
    }
}
