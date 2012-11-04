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

        public ISkill Skill
        {
            get;
            set;
        }

        public string SkillName
        {
            get
            {
                return Skill.GetType().Name;
            }
        }

        public SkillType SkillType
        {
            get
            {
                // @todo: add more skill types
                if (Skill is CardTransformSkill || Skill is ActiveSkill)
                {
                    return SkillType.Active;
                }
                else if (Skill is TriggerSkill)
                {
                    return SkillType.Trigger;
                }
                else
                {
                    return SkillType.Enforced;
                }
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
                if (OnSelectedChanged != null)
                {
                    OnSelectedChanged(this, new EventArgs());
                }
                OnPropertyChanged("IsSelected");
            }
        }

        public event EventHandler OnSelectedChanged;

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
