using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;

namespace Sanguosha.UI.Controls
{
    public class PrivateDeckViewModel : ViewModelBase
    {
        public PrivateDeckViewModel()
        {
            Cards = new ObservableCollection<CardViewModel>();
            Cards.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Cards_CollectionChanged);
        }

        void  Cards_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
 	        OnPropertyChanged("DisplayText");
        }

        public string DisplayText
        {
            get
            {
                string s = Application.Current.TryFindResource(string.Format("Deck.{0}.Name", Name)) as string;
                if (s == null) s = string.Empty;
                s = string.Format("{0}[{1}]", s, Cards.Count);
                return s;
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("DisplayText");
            }
        }

        public ObservableCollection<CardViewModel> Cards
        {
            get;
            private set;
        }
    }
}
