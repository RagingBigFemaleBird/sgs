using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Sanguosha.UI.Controls
{
    public class CardChoiceLineViewModel : ViewModelBase
    {
        public CardChoiceLineViewModel()
        {
            cards = new ObservableCollection<CardViewModel>();
        }

        private string deckName;
        public string DeckName
        {
            get { return deckName; }
            set
            {
                if (deckName == value) return;
                deckName = value;
                OnPropertyChanged("DeckName");
            }
        }

        ObservableCollection<CardViewModel> cards;
        public ObservableCollection<CardViewModel> Cards
        {
            get
            {
                return cards;
            }
            private set
            {
                if (cards == value) return;
                cards = value;
                OnPropertyChanged("Cards"); 
            }
        }
    }

    public class CardChoiceViewModel : ViewModelBase
    {
        public CardChoiceViewModel()
        {
            cardStacks = new ObservableCollection<CardChoiceLineViewModel>();
        }

        private string prompt;
        public string Prompt
        {
            get { return prompt; }
            set
            {
                if (prompt == value) return;
                prompt = value;
                OnPropertyChanged("Prompt");
            }
        }


        private bool canClose;

        public bool CanClose
        {
            get { return canClose; }
            set
            {
                if (canClose == value) return;
                canClose = value;
                OnPropertyChanged("CanClose");
            }
        }

        private ObservableCollection<CardChoiceLineViewModel> cardStacks;
        public ObservableCollection<CardChoiceLineViewModel> CardStacks
        {
            get { return cardStacks; }
            set
            {
                if (cardStacks == value) return;
                cardStacks = value;
                OnPropertyChanged("CardStacks");
            }
        }

        private double _timeOutSeconds;
        public double TimeOutSeconds
        {
            get
            {
                return _timeOutSeconds;
            }
            set
            {
                if (_timeOutSeconds == value) return;
                _timeOutSeconds = value;
                OnPropertyChanged("TimeOutSeconds");
            }
        }
    }
}
