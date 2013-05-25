using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using Sanguosha.Core.Cards;

namespace Sanguosha.UI.Controls
{
    public class SpecialDeckViewModel : ViewModelBase
    {
        public SpecialDeckViewModel()
        {
            Cards = new ObservableCollection<CardViewModel>();
            Cards.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Cards_CollectionChanged);
        }

        void  Cards_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
 	        OnPropertyChanged("DisplayText");
        }

        /// <summary>
        /// Localized translation of the deck name with number of cards to be displayed.
        /// </summary>
        public string DisplayText
        {
            get
            {                
                return string.Format("{0}[{1}]", TranslatedName, Cards.Count);             
            }
        }

        /// <summary>
        /// Localized translation of the deck name.
        /// </summary>
        public string TranslatedName
        {
            get
            {
                string s = Application.Current.TryFindResource(string.Format("Deck.{0}.Name", Name)) as string;
                return s??string.Empty;                
            }
        }
                
        public string Name
        {
            get
            {
                if (DeckPlace == null || DeckPlace.DeckType == null) return string.Empty;
                return DeckPlace.DeckType.Name;
            }            
        }

        private DeckPlace _deckPlace;

        public DeckPlace DeckPlace
        {
            get
            {
                return _deckPlace;
            }
            set
            {
                if (_deckPlace == value) return;
                _deckPlace = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("TranslatedName");
                OnPropertyChanged("DisplayText");
            }
        }


        public int? NumberOfCardsLimit
        {
            get;
            set;
        }
        

        public ObservableCollection<CardViewModel> Cards
        {
            get;
            private set;
        }
    }
}
