using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Heroes;
using System.Collections.ObjectModel;

namespace Sanguosha.UI.Controls
{
    public class HeroViewModel
    {
        public HeroViewModel()
        {
            SkillNames = new ObservableCollection<string>();
        }

        public HeroViewModel(Hero hero) : this()
        {
            Hero = hero;
        }

        private Hero _hero;
        public Hero Hero
        {
            get
            {
                return _hero;
            }
            set
            {
                if (_hero == value) return;
                _hero = value;
                SkillNames.Clear();
                if (_hero == null) return;
                foreach (var skill in Hero.Skills)
                {
                    SkillNames.Add(skill.GetType().Name);
                }
            }
        }

        public string Name
        {
            get
            {
                return Hero.Name;
            }
        }

        public Allegiance Allegiance
        {
            get
            {
                return Hero.Allegiance;
            }
        }

        public int MaxHealth
        {
            get
            {
                return Hero.MaxHealth;
            }
        }

        /// <summary>
        /// Returns the skill names of the primary hero.
        /// </summary>
        /// <remarks>For displaying tooltip purposes.</remarks>
        public ObservableCollection<string> SkillNames
        {
            get;
            private set;
        }
    }
}
