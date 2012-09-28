using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Skills;

namespace Sanguosha.Core.Heroes
{
    public enum Allegiance
    {
        Unknown,
        Shu,
        Wei,
        Wu,
        Qun,
        God
    }

    public class Hero
    {
        private Allegiance allegiance;

        public Allegiance Allegiance
        {
            get { return allegiance; }
            set { allegiance = value; }
        }
        private List<ISkill> skills;

        public List<ISkill> Skills
        {
            get { return skills; }
            set { skills = value; }
        }
        public Hero(Allegiance a, List<ISkill> s)
        {
            allegiance = a;
            skills = s;
        }
        public Hero(Allegiance a, params ISkill[] skills)
        {
            this.skills = new List<ISkill>(skills);
        }
    }
}
