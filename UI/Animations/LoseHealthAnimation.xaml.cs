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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sanguosha.UI.Animations
{
    /// <summary>
    /// Interaction logic for LoseHealthAnimation.xaml
    /// </summary>
    public partial class LoseHealthAnimation : UserControl
    {
        public LoseHealthAnimation()
        {
            InitializeComponent();
            Storyboard mainAnimation = Resources["mainAnimation"] as Storyboard;
            mainAnimation.Completed += new EventHandler(mainAnimation_Completed);
        }

        void mainAnimation_Completed(object sender, EventArgs e)
        {
            EventHandler handle = Completed;
            if (handle != null)
            {
                handle(this, e);
            }
        }
        
        public event EventHandler Completed;

    }
}
