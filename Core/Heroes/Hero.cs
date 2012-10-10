using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;

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
        public Allegiance Allegiance { get; set; }

        public List<ISkill> Skills { get; set; }

        public Player Owner { get; set; }

        public Hero(string name, Allegiance a, List<ISkill> skills)
        {
            Allegiance = a;
            Skills = skills;
            Name = name;
        }
        public Hero(string name, Allegiance a, params ISkill[] skills)
        {
            Allegiance = a;
            Skills = new List<ISkill>(skills);
            Name = name;
        }

        public string Name { get; set; }
    }
}
