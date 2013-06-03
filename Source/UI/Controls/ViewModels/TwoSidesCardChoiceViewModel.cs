using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sanguosha.UI.Controls
{
    public class TwoSidesCardChoiceViewModel : ViewModelBase
    {
        private string _prompt;
        public string Prompt
        {
            get { return _prompt; }
            set
            {
                if (_prompt == value) return;
                _prompt = value;
                OnPropertyChanged("Prompt");
            }
        }

        private ObservableCollection<CardViewModel> _cardsToPick;

        public ObservableCollection<CardViewModel> CardsToPick
        {
            get
            {
                return _cardsToPick;
            }
            set
            {
                if (_cardsToPick == value) return;
                _cardsToPick = value;
                OnPropertyChanged("CardsToPick");
            }
        }

        public List<CardViewModel> CardsPicked1
        {
            get;
            private set;
        }

        public List<CardViewModel> CardsPicked2
        {
            get;
            private set;
        }
    }
}
