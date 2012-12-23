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
    }
}
