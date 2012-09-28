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

        protected Allegiance Allegiance
        {
            get { return allegiance; }
            set { allegiance = value; }
        }
        private List<ISkill> skills;

        protected List<ISkill> Skills
        {
            get { return skills; }
            set { skills = value; }
        }
    }
}
