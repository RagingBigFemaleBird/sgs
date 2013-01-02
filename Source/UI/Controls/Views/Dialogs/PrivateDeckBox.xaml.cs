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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for PrivateDeckBox.xaml
    /// </summary>
    public partial class PrivateDeckBox : UserControl
    {
        public PrivateDeckBox()
        {
            InitializeComponent();
            cardStack.ParentCanvas = cardCanvas;
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(PrivateDeckBox_DataContextChanged);
        }

        void PrivateDeckBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var model = DataContext as IList<CardViewModel>;
            if (model == null) return;
            foreach (var card in model)
            {
                CardView view = new CardView(card);
                cardStack.Cards.Add(view);
            }
            cardStack.RearrangeCards();
        }
    }
}
