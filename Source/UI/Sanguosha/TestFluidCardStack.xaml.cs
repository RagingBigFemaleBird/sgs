using System;
using System.Collections.Generic;
using System.Linq;
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
using Sanguosha.UI.Controls;

namespace Sanguosha.UI.Main
{
    /// <summary>
    /// Interaction logic for TestFluidCardStack.xaml
    /// </summary>
    public partial class TestFluidCardStack : Window
    {
        public TestFluidCardStack()
        {
            InitializeComponent();
            GameEngine.LoadExpansions("./");
            var model = new CardViewModel() { Card = GameEngine.CardSet.First() };
            cardView1.DataContext = model;
        }
    }
}
