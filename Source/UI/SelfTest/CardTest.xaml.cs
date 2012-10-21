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
using System.Windows.Media.Animation;

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
            anim1 = new DoubleAnimation(0.0d, 100.0d, TimeSpan.FromMinutes(0.3d));
            Storyboard.SetTarget(anim1, rec);
            Storyboard.SetTargetProperty(anim1, new PropertyPath(Canvas.LeftProperty));
            sb = new Storyboard();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        DoubleAnimation anim1;
        Storyboard sb;

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            sb.Stop();
            sb.Children.Clear();
            sb.Children.Add(anim1);
            sb.Begin();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            anim1.To = 0.0d;
            anim1.Duration = TimeSpan.FromMinutes(0.2d);
            sb.Begin();

        }
    }
}
