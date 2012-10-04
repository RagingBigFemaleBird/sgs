using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Skills;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Core.Players
{
    [Serializable]
    public class Player
    {
        public Player()
        {
            id = 0;
            isMale = false;
            isFemale = false;
            maxHealth = 0;
            health = 0;
            hero = hero2 = null;
            attributes = new Dictionary<string, int>();
            AutoResetAttributes = new List<string>();
        }

        int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        bool isMale;

        public bool IsMale
        {
            get { return isMale; }
            set { isMale = value; }
        }

        bool isFemale;

        public bool IsFemale
        {
            get { return isFemale; }
            set { isFemale = value; }
        }

        int maxHealth;

        public int MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; }
        }

        int health;

        public int Health
        {
            get { return health; }
            set { if (health >= maxHealth) health = maxHealth; else health = value; }
        }

        Dictionary<string, int> attributes;

        /// <summary>
        /// 回合结束阶段过后自动清零的属性名称。
        /// </summary>
        public List<string> AutoResetAttributes { get; set; }

        public int this[string key]
        {
            get
            {
                if (!attributes.ContainsKey(key))
                {
                    return 0;
                }
                else
                {
                    return attributes[key];
                }
            }
            set
            {
                attributes[key] = value;
            }
        }

        private void SetHero(ref Hero hero, Hero value)
        {
            hero = value;
            if (hero != null)
            {
                foreach (ISkill skill in hero.Skills)
                {
                    skill.Owner = this;
                }
                Trace.Assert(hero.Owner == null);
                hero.Owner = this;
            }
        }

        private Hero hero;

        public Hero Hero
        {
            get { return hero; }
            set 
            {
                SetHero(ref hero, value);
            }
        }

        private Hero hero2;

        public Hero Hero2
        {
            get { return hero2; }
            set
            {
                SetHero(ref hero2, value);
            }
        }

        public List<ISkill> Skills
        {
            get
            {
                List<ISkill> s = new List<ISkill>();
                if (Hero != null)
                {
                    s.AddRange(Hero.Skills);
                }
                if (Hero2 != null)
                {
                    s.AddRange(Hero2.Skills);
                }
                return s;
            }
        }
    }
}
