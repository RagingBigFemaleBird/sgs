using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Sanguosha.UI.Controls
{
    public class WuGuChoiceViewModel : ViewModelBase
    {
        public WuGuChoiceViewModel()
        {
            _cards1 = new ObservableCollection<CardViewModel>();
            _cards2 = new ObservableCollection<CardViewModel>();
        }

        private bool _isEnabled;

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
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

        private ObservableCollection<CardViewModel> _cards1;

        public ObservableCollection<CardViewModel> Cards1
        {
            get
            {
                return _cards1;
            }
            set
            {
                if (_cards1 == value) return;
                _cards1 = value;
                OnPropertyChanged("Cards1");
            }
        }

        private ObservableCollection<CardViewModel> _cards2;

        public ObservableCollection<CardViewModel> Cards2
        {
            get
            {
                return _cards2;
            }
            set
            {
                if (_cards2 == value) return;
                _cards2 = value;
                OnPropertyChanged("Cards2");
            }
        }

        public IEnumerable<CardViewModel> Cards
        {
            get
            {
                return Cards1.Concat(Cards2);
            }
        }
    }
}
