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
    public partial class RevealRoleAnimation : FrameBasedAnimation
    {
        static List<ImageSource> frames;

        static RevealRoleAnimation()
        {
            frames = LoadFrames("pack://application:,,,/Animations;component/RevealRoleAnimation", 7);
        }

        public RevealRoleAnimation()
        {
            
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
