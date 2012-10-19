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

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
