using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for TwoSidesCardChoice.xaml
    /// </summary>
    public partial class TwoSidesCardChoiceBox : UserControl
    {
        public TwoSidesCardChoiceBox()
        {
            InitializeComponent();
            this.DataContextChanged += TwoSidesCardChoiceBox_DataContextChanged;
        }

        void TwoSidesCardChoiceBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldModel = e.OldValue as TwoSidesCardChoiceViewModel;
            if (oldModel != null)
            {
                oldModel.PropertyChanged -= model_PropertyChanged;
            }
            UpdateModel();
            var newModel = e.NewValue as TwoSidesCardChoiceViewModel;
            if (newModel != null)
            {
                newModel.PropertyChanged += model_PropertyChanged;
            }
        }

        private void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as TwoSidesCardChoiceViewModel;
            if (e.PropertyName == "TimeOutSeconds1")
            {
                StartCountDown(true, (int)model.TimeOutSeconds1);
            }
            else if (e.PropertyName == "TimeOutSeconds2")
            {
                StartCountDown(false, (int)model.TimeOutSeconds2);
            }
        }

        public void StartCountDown(bool isMainPlayer, int timeOutSeconds)
        {
            ProgressBar progressBar = isMainPlayer ? progressBar1 : progressBar2;

            if (timeOutSeconds == 0)
            {
                progressBar.Visibility = System.Windows.Visibility.Hidden;
                progressBar.BeginAnimation(ProgressBar.ValueProperty, null);
            }
            else
            {
                progressBar.Visibility = System.Windows.Visibility.Visible;
                progressBar.Opacity = 1.0d;

                Duration duration = new Duration(TimeSpan.FromSeconds(timeOutSeconds));
                DoubleAnimation doubleanimation = new DoubleAnimation(100d, 0d, duration);
                progressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);

            }
        }

        public void UpdateModel()
        {
            var model = DataContext as TwoSidesCardChoiceViewModel;
            if (model == null) return;
            spCardPicks1.Children.Clear();
            foreach (var card in model.CardsPicked1)
            {
                var cv = CardView.CreateCard(card, spCardPicks1, Settings.TwoSidesCardChoiceBox.CardWidth, Settings.TwoSidesCardChoiceBox.CardHeight);
                cv.Opacity = 1.0d;
                cv.Margin = new Thickness(5.0d);
            }
            spCardPicks2.Children.Clear();
            foreach (var card in model.CardsPicked2)
            {
                var cv = CardView.CreateCard(card, spCardPicks2, Settings.TwoSidesCardChoiceBox.CardWidth, Settings.TwoSidesCardChoiceBox.CardHeight);
                cv.Opacity = 1.0d;
                cv.Margin = new Thickness(5.0d);
            }
            ugCardsRepo.Children.Clear();
            foreach (var card in model.CardsToPick)
            {
                var cv = CardView.CreateCard(card, ugCardsRepo, Settings.TwoSidesCardChoiceBox.CardWidth, Settings.TwoSidesCardChoiceBox.CardHeight);
                cv.Opacity = 1.0d;
                cv.Margin = new Thickness(5.0d);
                cv.OffsetOnSelect = false;
            }
        }

        /// <summary>
        /// Pick a card from card repo to the row on the specified player's side.
        /// </summary>
        /// <param name="isMainPlayer"></param>
        /// <param name="cardIndex"></param>
        public void PickCard(bool isMainPlayer, int cardIndex)
        {
            var model = DataContext as TwoSidesCardChoiceViewModel;
            Trace.Assert(model != null);
            Trace.Assert(cardIndex >= 0 && cardIndex < ugCardsRepo.Children.Count);
            CardView card = ugCardsRepo.Children[cardIndex] as CardView;
            Trace.Assert(card != null);
            Point p = card.TranslatePoint(new Point(0, 0), canvasCards);
            var copy = CardView.CreateCard(card.CardModel, canvasCards, Settings.TwoSidesCardChoiceBox.CardWidth, Settings.TwoSidesCardChoiceBox.CardHeight);
            copy.SetCurrentPosition(p);
            copy.Opacity = 1.0d;
            int num; 
            CardView cv;
            if (isMainPlayer)
            {
                num = model.NumCardsPicked1 - 1;
                Trace.Assert(num >= 0 && num < spCardPicks1.Children.Count);
                cv = spCardPicks1.Children[num] as CardView;
            }
            else
            {
                num = model.NumCardsPicked2 - 1;
                Trace.Assert(num >= 0 && num < spCardPicks1.Children.Count);
                cv = spCardPicks2.Children[num] as CardView;
            }
            p = cv.TranslatePoint(new Point(0, 0), canvasCards);
            copy.Position = p;
            copy.Rebase();
            card.DataContext = new CardSlotViewModel();
        }
    }
}
