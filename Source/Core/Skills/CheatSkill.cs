using Sanguosha.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
