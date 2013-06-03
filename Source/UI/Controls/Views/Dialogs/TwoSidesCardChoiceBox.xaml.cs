using System;
using System.Collections.Generic;
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
            UpdateModel();
        }

        public void StartCountDown(bool isMainPlayer, int timeOutSeconds)
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(timeOutSeconds));
            DoubleAnimation doubleanimation = new DoubleAnimation(100d, 0d, duration);
            ProgressBar progressBar = isMainPlayer ? progressBar1 : progressBar2;
            progressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
        }

        public void UpdateModel()
        {
            var model = DataContext as TwoSidesCardChoiceViewModel;
            if (model == null) return;

            foreach (var card in model.CardsPicked1)
            {
                CardView.CreateCard(card, spCardPicks1, Settings.TwoSidesCardChoiceBox.CardWidth, Settings.TwoSidesCardChoiceBox.CardHeight);
            }

            foreach (var card in model.CardsPicked2)
            {
                CardView.CreateCard(card, spCardPicks2, Settings.TwoSidesCardChoiceBox.CardWidth, Settings.TwoSidesCardChoiceBox.CardHeight);
            }
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

        public void PickCard(bool isMainPlayer, int cardIndex)
        {
            var model = DataContext as TwoSidesCardChoiceViewModel;
            Trace.Assert(model != null);
            Trace.Assert(cardIndex >= 0 && cardIndex < model.CardsToPick.Count);
            CardView card = icCardRepo.ItemContainerGenerator.ContainerFromIndex(cardIndex) as CardView;
            Trace.Assert(card != null);
            Point p = card.TranslatePoint(new Point(0, 0), canvasCards);
            var copy = CardView.CreateCard(card.CardModel, canvasCards, Settings.TwoSidesCardChoiceBox.CardWidth, Settings.TwoSidesCardChoiceBox.CardHeight);
            copy.SetCurrentPosition(p);
            int num = _NumCardsPicked(model.CardsPicked1);
            CardView cv;
            if (isMainPlayer)
            {
                Trace.Assert(num >= 0 && num < spCardPicks1.Children.Count);
                cv = spCardPicks1.Children[num] as CardView;
            }
            else
            {
                Trace.Assert(num >= 0 && num < spCardPicks1.Children.Count);
                cv = spCardPicks2.Children[num] as CardView;
            }
            p = cv.TranslatePoint(new Point(0, 0), canvasCards);
            copy.Position = p;
            copy.Rebase();
        }
    }
}
