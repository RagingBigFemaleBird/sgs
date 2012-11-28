using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Skills;
using System.Windows.Input;

namespace Sanguosha.UI.Controls
{
    public enum SkillType
    {
        Active,
        Trigger,
        SingleUse,
        Enforced,
        Awakening,
    }

    public class SkillCommand : ViewModelBase, ICommand
    {
        #region Constructors
        public SkillCommand()
        {
        }
        #endregion // Constructors

        #region Fields

        private ISkill _skill;
        public ISkill Skill
        {
            get
            {
                return _skill;
            }
            set
            {
                if (_skill == value) return;
                _skill = value;
                // @todo: add more skill types
                if (value.IsEnforced)
                {
                    _skillType = SkillType.Enforced;
                }
                else if (value.IsSingleUse)
                {
                    _skillType = SkillType.SingleUse;
                }
                else if (value.IsAwakening)
                {
                    _skillType = SkillType.Awakening;
                }
                else if (value is CardTransformSkill || value is ActiveSkill)
                {
                    _skillType = SkillType.Active;
                }
                else if (value is TriggerSkill)
                {
                    var ts = value as TriggerSkill;
                    if (ts.IsAutoInvoked != null)
                    {
                        IsAutoInvokeSkill = true;
                        IsEnabled = true;
                        IsSelected = (ts.IsAutoInvoked == true);
                    }                   
                    _skillType = SkillType.Trigger;
                }
                else
                {
                    _skillType = SkillType.Enforced;
                }

                if (value is IRulerGivenSkill)
                {
                    var skill = value as IRulerGivenSkill;
                    HeroName = skill.Master.Hero.Name;
                }
            }
        }

        private bool _isAutoInvokeSkill;

        public bool IsAutoInvokeSkill
        {
            get { return _isAutoInvokeSkill; }
            set 
            {
                if (_isAutoInvokeSkill == value) return;
                _isAutoInvokeSkill = value;
                OnPropertyChanged("IsAutoInvokeSkill");
            }
        }

        private bool _isHighlighted;

        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                if (_isHighlighted == value) return;
                _isHighlighted = value;
                OnPropertyChanged("IsHighlighted");
            }
        }

        public string SkillName
        {
            get
            {
                return Skill.GetType().Name;
            }
        }

        public string HeroName
        {
            get;
            private set;
        }

        private SkillType _skillType;

        public SkillType SkillType
        {
            get
            {
                return _skillType;
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                SelectedChanged();
                OnPropertyChanged("IsSelected");
                if (!_isAutoInvokeSkill)
                {
                    IsHighlighted = value;
                }
                else
                {
                    var ts = Skill as TriggerSkill;
                    ts.IsAutoInvoked = value;
                }
            }
        }

        public event EventHandler OnSelectedChanged;

        protected virtual void SelectedChanged()
        {
            EventHandler handle = OnSelectedChanged;
            if (handle != null)
            {
                handle(this, new EventArgs());
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (isEnabled == value)
                {
                    return;
                }
                isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }
        #endregion

        #region ICommand
        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
        }
        #endregion
    }
}
