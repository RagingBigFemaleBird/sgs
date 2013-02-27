using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Linq;
using Sanguosha.Core.Cards;
using System.Collections.ObjectModel;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for CardChoiceBox.xaml
    /// </summary>
    public partial class CardChoiceBox : UserControl
    {
        public CardChoiceBox()
        {
            this.InitializeComponent();
            this.DataContextChanged += CardChoiceBox_DataContextChanged;
        }

        void CardChoiceBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateModel();
        }

        private static double _cardXSpacing = 100;
        private static double _cardYSpacing = 150;

        private void card_MouseEnter(object sender, EventArgs args)
        {
            var currentCard = sender as CardView;
            if (!currentCard.IsEnabled) return;
            if (currentCard.CardModel != null)
            {

                CardChoiceViewModel model = DataContext as CardChoiceViewModel;
                var allCards = from stack in model.CardStacks select stack.Cards;
                foreach (var cards in allCards)
                {
                    foreach (var card in cards)
                    {
                        if (card == currentCard.CardModel)
                        {
                            if (currentCard.CardModel.IsEnabled)
                            {
                                currentCard.CardModel.IsFaded = false;
                            }
                        }
                        else
                        {
                            card.IsFaded = true;
                        }
                    }
                }
            }
        }

        private void card_MouseLeave(object sender, EventArgs args)
        {
            var currentCard = sender as CardView;

            CardChoiceViewModel model = DataContext as CardChoiceViewModel;
            var allCards = from stack in model.CardStacks select stack.Cards;
            foreach (var cards in allCards)
            {
                foreach (var card in cards)
                {
                    card.IsFaded = !card.IsEnabled;
                }
            }

        }

        private void UpdateModel()
        {
            CardChoiceViewModel model = DataContext as CardChoiceViewModel;
            if (model == null) return;

            _cardStacks.Children.Clear();
            _canvas.Children.Clear();

            if (model.CardStacks.Count == 0)
            {
                return;
            }

            int maxCount = (from line in model.CardStacks
                            select line.Cards.Count).Max();

            _cardStacks.Width = Math.Min(maxCount * _cardXSpacing, 570);
            _cardStacks.Height = model.CardStacks.Count * _cardYSpacing;

            ObservableCollection<string> deckNames = new ObservableCollection<string>();

            // First, create layout.
            foreach (var line in model.CardStacks)
            {
                deckNames.Add(line.DeckName);

                CardStack stack = new SingleRowCardStack() { ParentCanvas = _canvas };
                stack.MaxCardSpacing = _cardXSpacing;
                stack.MaxCardSpacingOnHighlighted = _cardXSpacing + 15;
                stack.CardAlignment = HorizontalAlignment.Center;
                stack.Height = 130d;
                stack.Margin = new Thickness(1, 10, 1, 10);
                stack.AddCards(line.Cards);
                foreach (var card in stack.Cards)
                {
                    card.Cursor = Cursors.Hand;
                    card.MouseEnter += card_MouseEnter;
                    card.MouseLeave += card_MouseLeave;
                }

                stack.HorizontalAlignment = HorizontalAlignment.Stretch;
                _cardStacks.Children.Add(stack);
            }

            deckIcons.ItemsSource = deckNames;

            if (model == null || model.DisplayOnly) return;
            Trace.Assert(model != null);

            foreach (var s in model.CardStacks)
            {
                foreach (var card in s.Cards)
                {
                    if (model.Verifier.Verify(new List<List<Card>>() { new List<Card>() { card.Card } })
                        != Core.UI.VerifierResult.Success)
                    {
                        card.IsEnabled = false;
                    }

                    card.OnSelectedChanged += (o, e) =>
                    {
                        var c = o as CardViewModel;
                        model.Answer.Clear();
                        model.Answer.Add(new List<Card>());
                        if (c.IsSelected)
                        {
                            model.Answer[0].Add(c.Card);
                            foreach (var otherCardStack in model.CardStacks)
                            {
                                foreach (var otherCard in otherCardStack.Cards)
                                {
                                    if (otherCard != o)
                                    {
                                        otherCard.IsSelected = false;
                                    }
                                }
                            }
                            if (model.MultiChoiceCommands.Count == 1)
                            {
                                model.MultiChoiceCommands.First().Execute(null);
                            }
                            else
                            {
                                foreach (var command in model.MultiChoiceCommands)
                                {
                                    MultiChoiceCommand mc = command as MultiChoiceCommand;
                                    mc.CanExecuteStatus = true;
                                }
                            }
                        }
                        else
                        {
                            foreach (var command in model.MultiChoiceCommands)
                            {
                                MultiChoiceCommand mc = command as MultiChoiceCommand;
                                mc.CanExecuteStatus = false;
                            }
                        }
                    };
                }
            }

            if (model.MultiChoiceCommands.Count > 1)
            {
                gridChoices.Visibility = Visibility.Visible;
            }
            else
            {
                Trace.Assert(model.MultiChoiceCommands.Count == 1);
                gridChoices.Visibility = Visibility.Hidden;
            }

            if (model.TimeOutSeconds > 0)
            {
                Duration duration = new Duration(TimeSpan.FromSeconds(model.TimeOutSeconds));
                DoubleAnimation doubleanimation = new DoubleAnimation(100d, 0d, duration);
                progressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
            }
        }
    }
}