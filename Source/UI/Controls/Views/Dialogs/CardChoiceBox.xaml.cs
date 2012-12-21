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

        private void UpdateModel()
        {
            var model = DataContext as CardChoiceViewModel;
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
                        if (c.IsSelected)
                        {
                            model.Answer.Add(new List<Card>() { c.Card });
                            if (model.MultiChoiceCommands.Count == 1)
                            {
                                model.MultiChoiceCommands.First().Execute(null);
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