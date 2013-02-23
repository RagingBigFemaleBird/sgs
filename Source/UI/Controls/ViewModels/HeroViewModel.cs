using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Heroes;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;

namespace Sanguosha.UI.Controls
{
    public class HeroViewModel : ViewModelBase
    {
        public HeroViewModel()
        {
            SkillNames = new ObservableCollection<string>();
            SkillCommands = new ObservableCollection<SkillCommand>();
            SkillNames = new ObservableCollection<string>();
            heroNameChars = new ObservableCollection<string>();
        }

        public HeroViewModel(Hero hero)
            : this()
        {
            Hero = hero;
        }

        private PropertyChangedEventHandler _PropertyChanged;

        private void _OnHeroPropertyChanged(object o, PropertyChangedEventArgs e)
        {
            string name = e.PropertyName;
            if (name == "Allegiance")
            {
                OnPropertyChanged("Allegiance");
            }
            else if (name == "Skills")
            {
            }
            else
            {
                throw new NotImplementedException(string.Format("HeroViewModel didn't expect change on property {0}", name));
            }
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
                if (_hero != null)
                {
                    _hero.PropertyChanged -= _PropertyChanged;
                }
                _hero = value;
                if (_hero != null)
                {
                    _PropertyChanged = _OnHeroPropertyChanged;
                    _hero.PropertyChanged += _PropertyChanged;
                    _UpdateHeroInfo();
                    OnPropertyChanged("Name");
                    OnPropertyChanged("IsMale");
                    OnPropertyChanged("Allegiance");
                    OnPropertyChanged("MaxHealth");
                }
            }
        }

        private void _UpdateHeroInfo()
        {
            SkillNames.Clear();
            heroNameChars.Clear();

            if (Hero != null)
            {
                string name = Application.Current.TryFindResource(string.Format("Hero.{0}.Name", Hero.Name)) as string;
                if (name != null)
                {
                    foreach (var heroChar in name)
                    {
                        if (heroNameChars.Count > 0 && (char.IsUpper(heroChar) || char.IsLower(heroChar)) &&
                            (char.IsUpper(heroNameChars.Last().Last()) || char.IsUpper(heroNameChars.Last().Last())))
                        {
                            heroNameChars[heroNameChars.Count - 1] += heroChar;
                        }
                        else
                        {
                            heroNameChars.Add(heroChar.ToString());
                        }
                    }
                }
                foreach (var skill in Hero.Skills)
                {
                    SkillNames.Add(skill.GetType().Name);
                }
            }
        }

        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get
            {
                if (Hero == null) return string.Empty;
                return Hero.Name;
            }
        }

        ObservableCollection<string> heroNameChars;

        public ObservableCollection<string> NameChars
        {
            get
            {
                return heroNameChars;
            }
        }

        public bool IsMale
        {
            get
            {
                if (Hero == null) return false;
                return Hero.IsMale;
            }
        }

        public Allegiance Allegiance
        {
            get
            {
                if (Hero == null) return Core.Heroes.Allegiance.Unknown;
                return Hero.Allegiance;
            }
        }

        public int MaxHealth
        {
            get
            {
                if (Hero == null) return 0;
                return Hero.MaxHealth;
            }
        }

        public string ExpansionName
        {
            get;
            set;
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


        public ObservableCollection<SkillCommand> SkillCommands
        {
            get;
            private set;
        }

        private string _impersonatedHeroName;

        public string ImpersonatedHeroName
        {
            get { return _impersonatedHeroName; }
            set 
            {
                if (_impersonatedHeroName == value) return;
                _impersonatedHeroName = value;
                OnPropertyChanged("ImpersonatedHeroName");
            }
        }

        private string _impersonatedSkill;

        public string ImpersonatedSkill
        {
            get { return _impersonatedSkill; }
            set
            {
                if (_impersonatedSkill == value) return;
                _impersonatedSkill = value;
                OnPropertyChanged("ImpersonatedSkill");
            }
        }

        internal void UpdateSkillNames()
        {
            SkillNames.Clear();
            foreach (var skill in SkillCommands)
            {
                SkillNames.Add(skill.Skill.GetType().Name);
            }
        }
    }
}
