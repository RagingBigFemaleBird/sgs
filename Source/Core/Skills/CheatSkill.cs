using Sanguosha.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.UI;

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
        }
        public CheatType CheatType { get; set; }
        public int CardId { get; set; }
        public string SkillName { get; set; }
        [NonSerialized]
        public Players.Player owner;

        public Players.Player Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public DeckType ExtraCardsDeck
        {
            get { return null; }
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

        public UiHelper Helper
        {
            get { return helper; }
            private set { helper = value; }
        }
    }
}
