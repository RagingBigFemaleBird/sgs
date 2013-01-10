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
            _cards = new ObservableCollection<CardViewModel>();
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

        private ObservableCollection<CardViewModel> _cards;

        public ObservableCollection<CardViewModel> Cards
        {
            get
            {
                return _cards;
            }
            set
            {
                if (_cards == value) return;
                _cards = value;
                OnPropertyChanged("Cards");
            }
        }
    }
}
