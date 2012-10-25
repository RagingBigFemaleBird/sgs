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
        }

        private void border3_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
        	border3.Margin = new Thickness(10, 0, 10, 0);
			// TODO: Add event handler implementation here.
        }

        private void border3_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
			border3.Margin = new Thickness(0, 0, 0, 0);
        	// TODO: Add event handler implementation here.
        }
    }
}
