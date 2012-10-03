using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            Hero = Hero2 = null;
            attributes = new Dictionary<string, int>();
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
            set { if (health > maxHealth) health = maxHealth; else health = value; }
        }

        Dictionary<string, int> attributes;

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

        public Hero Hero { get; set; }
        public Hero Hero2 { get; set; }

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
