using Sanguosha.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.UI;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Core.Skills
{
    [Serializable]
    public enum CheatType
    {
        Card,
        Skill,
    }

    [Serializable]
    public class CheatSkill : ISkill
    {
        public CheatSkill()
        {
            Helper = new UiHelper();
            HeroTag = null;
        }
        public CheatType CheatType { get; set; }
        public int CardId { get; set; }
        public string SkillName { get; set; }
        [NonSerialized]
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

        [NonSerialized]
        UiHelper helper;

        [NonSerialized]
        private Hero heroTag;

        public Hero HeroTag
        {
            get { return heroTag; }
            set { heroTag = value; }
        }

        public UiHelper Helper
        {
            get { return helper; }
            private set { helper = value; }
        }
    }
}
