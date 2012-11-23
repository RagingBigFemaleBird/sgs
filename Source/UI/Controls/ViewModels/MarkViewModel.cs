using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Sanguosha.Core.Players;

namespace Sanguosha.UI.Controls
{
    public class MarkViewModel : ViewModelBase
    {
        public MarkViewModel()
        {
            _digits = new ObservableCollection<int>();
        }

        private PlayerAttribute _attribute;

        public PlayerAttribute PlayerAttribute
        {
            get
            {
                return _attribute;
            }
            set
            {
                if (_attribute == value) return;
                _attribute = value;
                _markName = _attribute.Name;
            }
        }

        public int _number;

        public int Number
        {
            get
            {
                return _number;
            }
            set
            {
                if (_number == value) return;
                _number = value;
                OnPropertyChanged("Number");
                _digits.Clear();
                IsExisted = (_number > 0);
                if (_number > 1)
                {
                    int num = _number;
                    while (num > 0)
                    {
                        _digits.Insert(0, num % 10);
                        num /= 10;
                    }
                }
            }
        }

        private bool _isExisted;

        public bool IsExisted
        {
            get
            {
                return _isExisted;
            }
            set
            {
                if (_isExisted == value) return;
                _isExisted = value;
                OnPropertyChanged("IsExisted");
            }
        }

        private string _markName;

        public string MarkName
        {
            get
            {
                return _markName;
            }
        }

        ObservableCollection<int> _digits;
        public ObservableCollection<int> Digits
        {
            get
            {
               return _digits; 
            }
        }
    }
}
