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

namespace Sanguosha.UI.Selftest
{
    /// <summary>
    /// Interaction logic for StackPanelTest.xaml
    /// </summary>
    public partial class StackPanelTest : Window
    {
        public StackPanelTest()
        {
            InitializeComponent();
        }
        private void changeOrientation(object sender, SelectionChangedEventArgs args)
        {
            ListBoxItem li = ((sender as ListBox).SelectedItem as ListBoxItem);
            if (li.Content.ToString() == "Horizontal")
            {
                sp1.Orientation = System.Windows.Controls.Orientation.Horizontal;
            }
            else if (li.Content.ToString() == "Vertical")
            {
                sp1.Orientation = System.Windows.Controls.Orientation.Vertical;
            }

        }

        private void changeHorAlign(object sender, SelectionChangedEventArgs args)
        {
            ListBoxItem li = ((sender as ListBox).SelectedItem as ListBoxItem);
            if (li.Content.ToString() == "Left")
            {
                sp1.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            }
            else if (li.Content.ToString() == "Right")
            {
                sp1.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            }
            else if (li.Content.ToString() == "Center")
            {
                sp1.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            }
            else if (li.Content.ToString() == "Stretch")
            {
                sp1.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            }
        }

        private void changeVertAlign(object sender, SelectionChangedEventArgs args)
        {
            ListBoxItem li = ((sender as ListBox).SelectedItem as ListBoxItem);
            if (li.Content.ToString() == "Top")
            {
                sp1.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            }
            else if (li.Content.ToString() == "Bottom")
            {
                sp1.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            }
            else if (li.Content.ToString() == "Center")
            {
                sp1.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            }
            else if (li.Content.ToString() == "Stretch")
            {
                sp1.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            }
        }

    }
}
