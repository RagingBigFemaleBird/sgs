using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Sanguosha.Core.Cards;
using System.Diagnostics;

namespace Sanguosha.UI.Controls
{
    public class EquipCommand : CardViewModel, ICommand
    {
        #region Constructors
        public EquipCommand()
        {
        }
        #endregion // Constructors

        #region Fields

        public Equipment Equipment
        {
            get
            {
                return Card.Type as Equipment;
            }
        }

        public override Card Card
        {
	        get 
	        { 
		         return base.Card;
	        }
	        set 
	        {
                if (base.Card == value) return;
                Equipment equip = value.Type as Equipment;
                if (equip == null)
                {
                    Trace.TraceWarning("Installing non-equipment in equipment area.");
                }
                _skillCommand = new SkillCommand() { Skill = equip.EquipmentSkill };
		        base.Card = value;
	        }
        }

        private SkillCommand _skillCommand;
        public SkillCommand SkillCommand
        {
            get
            {
                return _skillCommand;
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
