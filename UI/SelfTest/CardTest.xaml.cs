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
using System.Windows.Shapes;

using Sanguosha.Core.Games;
using Sanguosha.Core.Cards;
using Sanguosha.UI.Controls;

namespace Sanguosha.UI.Selftest
{
    /// <summary>
    /// Interaction logic for CardTest.xaml
    /// </summary>
    public partial class CardTest : Window
    {
        public CardTest()
        {
            InitializeComponent();
            GameEngine.LoadExpansions("Expansions");
            Game game = new RoleGame();
            foreach (var g in GameEngine.Expansions.Values)
            {
                game.LoadExpansion(g);
            }            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CardViewModel model = new CardViewModel();
            model.Card = GameEngine.CardSet[2];
            CardView cardView1 = new CardView();
            cardView1.DataContext = model;
            cardView1.Width = 93;
            cardView1.Height = 130;
            model.Footnote = "张角打出";

            CardViewModel model2 = new CardViewModel();
            model2.Card = GameEngine.CardSet[3];
            CardView cardView2 = new CardView();
            cardView2.DataContext = model2;
            cardView2.Width = 93;
            cardView2.Height = 130;
            model2.Footnote = "刘备吃掉";
            
            List<CardView> cards = new List<CardView>();
            cards.Add(cardView1);
            cards.Add(cardView2);
            // cardStack.CardAlignment = System.Windows.HorizontalAlignment.Left;
            cardStack.AddCards(cards, 0.5);
            
        }
    }
}
