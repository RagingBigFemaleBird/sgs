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
        public TwoSidesCardChoiceViewModel()
        {
            CardsToPick = new ObservableCollection<CardViewModel>();
            CardsPicked1 = new List<CardViewModel>();
            CardsPicked2 = new List<CardViewModel>();
            GroupOfPlayer = new Dictionary<int, int>();
        }

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

        public bool IsEnabled
        {
            get;
            set;
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

        private int _NumCardsPicked(IList<CardViewModel> allCards)
        {
            int i = 0;
            for (; i < allCards.Count; i++)
            {
                if (allCards[i] is CardSlotViewModel)
                {
                    break;
                }
            }
            return i;
        }
        public int NumCardsPicked1
        {
            get
            {
                return _NumCardsPicked(CardsPicked1);
            }
        }
        public int NumCardsPicked2
        {
            get
            {
                return _NumCardsPicked(CardsPicked2);
            }
        }

        private double _timeOutSeconds1;
        public double TimeOutSeconds1
        {
            get
            {
                return _timeOutSeconds1;
            }
            set
            {
                if (_timeOutSeconds1 == value) return;
                _timeOutSeconds1 = value;
                OnPropertyChanged("TimeOutSeconds1");
            }
        }

        private double _timeOutSeconds2;
        public double TimeOutSeconds2
        {
            get
            {
                return _timeOutSeconds2;
            }
            set
            {
                if (_timeOutSeconds2 == value) return;
                _timeOutSeconds2 = value;
                OnPropertyChanged("TimeOutSeconds2");
            }
        }

        public IDictionary<int, int> GroupOfPlayer
        {
            get;
            set;
        }
    }
}
