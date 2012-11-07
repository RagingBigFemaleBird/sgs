using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

using Sanguosha.Core.Skills;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Games;
using System.Collections.ObjectModel;

namespace Sanguosha.Core.Players
{
    [Serializable]
    public class Player : INotifyPropertyChanged
    {
        public Player()
        {
            isDead = false;
            id = 0;
            isMale = false;
            isFemale = false;
            maxHealth = 0;
            health = 0;
            hero = hero2 = null;
            attributes = new Dictionary<PlayerAttribute, int>();
            equipmentSkills = new List<ISkill>();
        }

        int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        bool isIronShackled;

        /// <summary>
        /// 铁锁
        /// </summary>
        public bool IsIronShackled
        {
            get { return isIronShackled; }
            set
            {
                if (isIronShackled == value) return;
                isIronShackled = value;
                OnPropertyChanged("IsIronShackled");
            }
        }

        bool isImprisoned;

        /// <summary>
        /// 翻面
        /// </summary>
        public bool IsImprisoned
        {
            get { return isImprisoned; }
            set
            {
                if (isImprisoned == value) return;
                isImprisoned = value;
                OnPropertyChanged("IsImprisoned");
            }
        }


        bool isDead;

        public bool IsDead
        {
            get { return isDead; }
            set 
            {
                if (isDead == value) return;
                isDead = value;
                OnPropertyChanged("IsDead");
            }
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
            set 
            {
                if (maxHealth == value)
                {
                    return;
                }
                maxHealth = value;
                OnPropertyChanged("MaxHealth");
            }
        }

        private Allegiance allegiance;

        public Allegiance Allegiance
        {
            get { return allegiance; }
            set
            {
                if (allegiance == value)
                {
                    return;
                }
                allegiance = value;
                OnPropertyChanged("Allegiance");
            }
        }

        int health;

        public int Health
        {
            get { return health; }
            set 
            {
                if (health == value)
                {
                    return;
                }
                health = value;
                OnPropertyChanged("Health");
            }
        }

        Dictionary<PlayerAttribute, int> attributes;

        public Dictionary<PlayerAttribute, int> Attributes
        {
            get { return attributes; }            
        }

        public int this[PlayerAttribute key]
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
                if (!attributes.ContainsKey(key))
                {
                    attributes.Add(key, value);
                }
                else if (attributes[key] == value)
                {
                    return;
                }
                attributes[key] = value;
                OnPropertyChanged("Attributes");
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
            OnPropertyChanged("Skills");
        }

        private Hero hero;

        public Hero Hero
        {
            get { return hero; }
            set 
            {
                if (hero == value) return;
                SetHero(ref hero, value);
                OnPropertyChanged("Hero");
            }
        }

        private Hero hero2;

        public Hero Hero2
        {
            get { return hero2; }
            set
            {
                if (hero2 == value) return;
                SetHero(ref hero2, value);
                OnPropertyChanged("Hero2");
            }
        }

        private Role role;

        public Role Role
        {
            get { return role; }
            set
            {
                if (role == value) return;
                role = value;
                OnPropertyChanged("Role");
            }
        }

        private List<ISkill> equipmentSkills;

        public IList<ISkill> EquipmentSkills
        {
            get { return new ReadOnlyCollection<ISkill>(equipmentSkills); }
        }

        public void AcquireEquipmentSkill(ISkill skill)
        {
            skill.Owner = this;
            equipmentSkills.Add(skill);
            OnPropertyChanged("Skills");
        }

        public void LoseEquipmentSkill(ISkill skill)
        {
            skill.Owner = null;
            equipmentSkills.Remove(skill);
            OnPropertyChanged("Skills");
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks> UI use only!</remarks>
        public IList<ISkill> Skills
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
                return new ReadOnlyCollection<ISkill>(s);
            }
        }

        public IList<ISkill> ActionableSkills
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
                s.AddRange(equipmentSkills);
                return new ReadOnlyCollection<ISkill>(s);
            }
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

        private bool isTargeted;

        public bool IsTargeted
        {
            get { return isTargeted; }
            set { isTargeted = value; OnPropertyChanged("IsTargeted"); }
        }

        public static PlayerAttribute RangeMinus = PlayerAttribute.Register("RangeMinus", false);
        public static PlayerAttribute RangePlus = PlayerAttribute.Register("RangePlus", false);
        public static PlayerAttribute AttackRange = PlayerAttribute.Register("AttackRange", false);
        public static PlayerAttribute DealAdjustment = PlayerAttribute.Register("DealAdjustment", true);
    }
}
