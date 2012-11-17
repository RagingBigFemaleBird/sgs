using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Sanguosha.Core.Cards;
using System.Windows.Input;
using Sanguosha.Core.Skills;

namespace Sanguosha.UI.Controls
{
    public class GuHuoSkillCommand : SkillCommand
    {
        public GuHuoSkillCommand()
        {
            guHuoTypes = new ObservableCollection<CardHandler>();
            guHuoCommand = new GuHuoChoiceCommand() { ParentSkillCommand = this };
        }

        class GuHuoChoiceCommand : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged
            {
                add { }
                remove { }
            }

            private GuHuoSkillCommand parentSkillCommand;

            public GuHuoSkillCommand ParentSkillCommand
            {
                get { return parentSkillCommand; }
                set { parentSkillCommand = value; }
            }

            public void Execute(object parameter)
            {
                CardHandler selection = parameter as CardHandler;
                parentSkillCommand.GuHuoChoice = selection;
            }
        }

        GuHuoChoiceCommand guHuoCommand;

        public ICommand GuHuoCommand
        {
            get { return guHuoCommand; }
        }

        public CardHandler GuHuoChoice
        {
            get 
            {
                var skill = Skill as IAdditionalTypedSkill;
                if (skill != null)
                {
                    return skill.AdditionalType;
                }
                return null;
            }
            set
            {
                var skill = Skill as IAdditionalTypedSkill;
                if (skill != null)
                {
                    skill.AdditionalType = value;
                }
            }
        }

        ObservableCollection<CardHandler> guHuoTypes;

        public ObservableCollection<CardHandler> GuHuoTypes
        {
            get { return guHuoTypes; }
            private set { guHuoTypes = value; }
        }
    }
}
