using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Sanguosha.UI.Animations
{
    public class AnimationBase : UserControl, IAnimation
    {
        protected Storyboard mainAnimation;

        public AnimationBase()
        {
            IsHitTestVisible = false;
        }

        public void Start()
        {
            if (mainAnimation == null)
            {
                if (!Resources.Contains("mainAnimation"))
                {
                    Trace.TraceError("Animation not found");
                    return;
                }
                mainAnimation = Resources["mainAnimation"] as Storyboard;
                if (mainAnimation == null) return;
            }
            mainAnimation.Completed += mainAnimation_Completed;
            StartMainAnimation();
        }

        void mainAnimation_Completed(object sender, EventArgs e)
        {
            Panel panel = this.VisualParent as Panel;
            if (panel != null && panel.Children.Contains(this))
            {
                panel.Children.Remove(this);
            }
            else
            {
                Trace.TraceError("Cannot find animation's parent canvas. Failed to remove animation");
            }
        }

        protected virtual void StartMainAnimation()
        {
            if (mainAnimation != null)
            {
                mainAnimation.Begin();
            }
        }
    }
}
