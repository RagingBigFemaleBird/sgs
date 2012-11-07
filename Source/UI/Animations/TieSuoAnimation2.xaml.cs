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

namespace Sanguosha.UI.Animations
{
    /// <summary>
    /// Interaction logic for TieSuoAnimation2.xaml
    /// </summary>
    public partial class TieSuoAnimation2 : FrameBasedAnimation
    {
        public TieSuoAnimation2()
        {
            InitializeComponent();
        }
        
        static List<ImageSource> frames;

        static TieSuoAnimation2()
        {
            frames = LoadFrames("pack://application:,,,/Animations;component/TieSuoAnimation2", 11);
        }

        public override List<ImageSource> Frames
        {
            get
            {
                return frames;
            }
        }
    }
}
