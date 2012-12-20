using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;

namespace Sanguosha.UI.Controls
{
    public class CardChoiceLineViewModel : ViewModelBase
    {
        public CardChoiceLineViewModel()
        {
            cards = new ObservableCollection<CardViewModel>();
            Capacity = -1;
        }

        private bool isResultDeck;

        public bool IsResultDeck
        {
            get { return isResultDeck; }
            set 
            {
                if (isResultDeck == value) return;
                isResultDeck = value;
                OnPropertyChanged("IsResultDeck");
            }
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

        private int capacity;

        public int Capacity
        {
            get { return capacity; }
            set 
            {
                if (capacity == value) return;
                capacity = value;
                OnPropertyChanged("Capacity");
            }
        }

        private bool rearrangable;

        public bool Rearrangable
        {
            get { return rearrangable; }
            set 
            {
                if (rearrangable == value) return;
                rearrangable = value;
                OnPropertyChanged("Rearrangable");
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
            MultiChoiceCommands = new ObservableCollection<ICommand>();
            Answer = new List<List<Card>>();
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

        public ObservableCollection<ICommand> MultiChoiceCommands
        {
            get;
            private set;
        }

        public List<List<Card>> Answer
        {
            get;
            private set;
        }

        public ICardChoiceVerifier Verifier { get; set; }
    }
}
