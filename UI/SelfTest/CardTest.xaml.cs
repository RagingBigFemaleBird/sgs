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
            CardViewModel model = new CardViewModel();
            model.Card = GameEngine.CardSet[2];
            
            cardView1.DataContext = model;
            model.Footnote = "张角打出";
        }
        double opacity = 0.8;
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // cardView1.IsFaded = true;
            opacity -= 0.2;
            cardView1.CardOpacity = opacity;
        }
    }
}
