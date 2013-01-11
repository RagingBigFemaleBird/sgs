using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.UI;

namespace Sanguosha.UI.Controls
{
    public class MultiChoiceCommand : SimpleRelayCommand
    {
        public MultiChoiceCommand(Action<object> execute) : base(execute)
        {
            CanExecuteStatus = true;
        }

        public override void Execute(object parameter)
        {
            base.Execute(choiceIndex);
        }

        public override bool CanExecute(object parameter)
        {
            return IsCancel || base.CanExecute(parameter);
        }

        private OptionPrompt choiceKey;
        public OptionPrompt ChoiceKey
        {
            get
            {
                return choiceKey;
            }
            set
            {
                if (choiceKey == value) return;
                choiceKey = value;
                OnPropertyChanged("ChoiceKey");
            }
        }

        private int choiceIndex;
        public int ChoiceIndex
        {
            get { return choiceIndex; }
            set
            {
                if (choiceIndex == value) return;
                choiceIndex = value;
                OnPropertyChanged("ChoiceIndex");
            }
        }

        /// <summary>
        /// Gets/sets if this command is the choice to cancel a multichoice.
        /// </summary>
        public bool IsCancel { get; set; }
    }
}
