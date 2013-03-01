using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using System.ComponentModel;

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

    public class Hero : ICloneable
    {
        private Allegiance allegiance;

        public Allegiance Allegiance
        {
            get { return allegiance; }
            set 
            {
                if (allegiance == value) return;
                allegiance = value;
                OnPropertyChanged("Allegiance");
            }
        }

        List<ISkill> _skills;
        public List<ISkill> Skills
        {
            get { return _skills; }
            set 
            { 
                _skills = value;
                OnPropertyChanged("Skills");
            }
        }

        public Player Owner { get; set; }

        public bool IsMale { get; set; }

        public int MaxHealth { get; set; }

        public String HeroConvertFrom { get; set; }

        public bool IsSpecialHero { get { return HeroConvertFrom != string.Empty; } }

        public Hero(string name, bool isMale, Allegiance a, int health, List<ISkill> skills)
        {
            Allegiance = a;
            Skills = skills;
            Name = name;
            MaxHealth = health;
            IsMale = isMale;
            HeroConvertFrom = string.Empty;
        }
        public Hero(string name, bool isMale, Allegiance a, int health, params ISkill[] skills)
        {
            Allegiance = a;
            Skills = new List<ISkill>(skills);
            Name = name;
            MaxHealth = health;
            IsMale = isMale;
            HeroConvertFrom = string.Empty;
        }

        public string Name { get; set; }

        public object Clone()
        {
            var hero = new Hero(Name, IsMale, Allegiance, MaxHealth, new List<ISkill>()) { HeroConvertFrom = this.HeroConvertFrom };
            hero.Skills = new List<ISkill>();
            foreach (var s in Skills)
            {
                hero.Skills.Add(s.Clone() as ISkill);
            }
            return hero;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public ISkill LoseSkill(string skillName)
        {
            foreach (var sk in Skills)
            {
                if (sk.GetType().Name == skillName)
                {
                    return LoseSkill(sk);
                }
            }
            return null;
        }

        public ISkill LoseSkill(ISkill skill)
        {
            if (!Skills.Contains(skill)) return null;
            Skills.Remove(skill);
            OnPropertyChanged("Skills");
            skill.HeroTag = null;
            skill.Owner = null;
            return skill;
        }

        public void LoseAllSkills()
        {
            if (Skills.Count == 0) return;
            List<ISkill> backup = new List<ISkill>(Skills);
            Skills.Clear();
            OnPropertyChanged("Skills");
            foreach (var sk in backup)
            {
                sk.HeroTag = null;
                sk.Owner = null;
            }
        }
    }
}
