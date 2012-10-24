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

        public void Start()
        {
            if (mainAnimation == null) return;
            mainAnimation.Completed += mainAnimation_Completed;
            StartMainAnimation();
        }

        void mainAnimation_Completed(object sender, EventArgs e)
        {
            Canvas canvas = this.VisualParent as Canvas;
            if (canvas != null && canvas.Children.Contains(this))
            {
                canvas.Children.Remove(this);
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
