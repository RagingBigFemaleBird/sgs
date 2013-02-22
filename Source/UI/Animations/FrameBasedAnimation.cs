using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sanguosha.UI.Animations
{
    public partial class FrameBasedAnimation : Image, IAnimation
    {
        public static readonly DependencyProperty ActiveFrameIndexProperty =
            DependencyProperty.Register("ActiveFrameIndex", typeof(int), typeof(FrameBasedAnimation));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(FrameBasedAnimation));

        public static readonly DependencyProperty FramesPerSecondProperty =
            DependencyProperty.Register("FramesPerSecond", typeof(double), typeof(FrameBasedAnimation), new PropertyMetadata(30d));

        public static readonly DependencyProperty WrapAroundProperty =
            DependencyProperty.Register("WrapAround", typeof(bool), typeof(FrameBasedAnimation));

        public ImageSource ActiveFrame 
        { 
            get 
            {
                return Frames[ActiveFrameIndex];
            } 
        }

        public int ActiveFrameIndex
        {
            get { return (int)GetValue(ActiveFrameIndexProperty); }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("The ActiveFrameIndex can not be negative.");

                if (value > MaximumFrameIndex)
                    throw new ArgumentOutOfRangeException("The ActiveFrameIndex can not be greater than MaximumFrameIndex.");

                SetValue(ActiveFrameIndexProperty, value);
                Source = ActiveFrame;
            }
        }

        public static List<ImageSource> LoadFrames(string folderPath, int totalNum)
        {
            List<ImageSource> result = new List<ImageSource>();
            for (int i = 0; i < totalNum; i++)
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(string.Format("{0}/{1}.png", folderPath, i));
                image.EndInit();
                
                result.Add(image);
            }
            return result;
        }

        public bool BypassFramesPerSecond { get; set; }
        public virtual List<ImageSource> Frames { get; private set; }

        public double FramesPerSecond
        {
            get { return (double)GetValue(FramesPerSecondProperty); }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("FramesPerSecond must be greater than 0.");

                SetValue(FramesPerSecondProperty, value);
            }
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set
            {
                // Activate.
                if (!IsActive && value)
                    CompositionTarget.Rendering += new System.EventHandler(CompositionTarget_Rendering);

                // Deactivate.
                if (IsActive && !value)
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;

                SetValue(IsActiveProperty, value);
            }
        }

        private TimeSpan LastRenderTime { get; set; }
        public int MaximumFrameIndex { get { return Frames.Count - 1; } }
        public int TotalFrames { get { return Frames.Count; } }

        public bool WrapAround
        {
            get { return (bool)GetValue(WrapAroundProperty); }
            set { SetValue(WrapAroundProperty, value); }
        }

        public FrameBasedAnimation()
        {
            Visibility = Visibility.Hidden;
            Frames = new List<ImageSource>();
            FramesPerSecond = 30;
            this.Loaded += FrameBasedAnimation_Loaded;
            this.Unloaded += FrameBasedAnimation_Unloaded;
        }

        void FrameBasedAnimation_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsActive)
            {
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
        }

        void FrameBasedAnimation_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;            
        }

        private void CompositionTarget_Rendering(object sender, System.EventArgs e)
        {
            TimeSpan timeSinceLastRender;

            // Enforce FramesPerSecond if BypassFramesPerSecond is false.
            timeSinceLastRender = (DateTime.Now.TimeOfDay - LastRenderTime);
            if (timeSinceLastRender.TotalSeconds < (1 / FramesPerSecond))
                return;

            LastRenderTime = DateTime.Now.TimeOfDay;

            // Set ActiveFrameIndex accordingly.
            if (ActiveFrameIndex < MaximumFrameIndex)
                ActiveFrameIndex++;
            else
            {
                if (WrapAround)
                    ActiveFrameIndex = 0;
                else
                    Stop();
            }
        }

        public event EventHandler Completed;

        public void Resume()
        {
            if (TotalFrames < 0)
                throw new Exception("FrameBasedAnimation can not start because it does not contain any frames.");

            IsActive = true;
        }

        public void Start()
        {
            if (IsActive) return;
            Visibility = Visibility.Visible;
            Resume();
            ActiveFrameIndex = 0;
        }

        public void Stop()
        {
            if (!IsActive) return;
            IsActive = false;
            Visibility = Visibility.Hidden;
            EventHandler handler = Completed;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}
