using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace wyDay.Controls
{
    public class AnimationControl : Image
    {
        BitmapSource m_BaseImage;
        int m_Rows = 1;
        int m_Columns = 1;
        bool m_SkipFirstFrame;

        //for static images
        bool staticImage;

        readonly float[][] ptsArray ={ 
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 0, 0, 0, 0}, 
            new float[] {0, 0, 0, 0, 1}};

        #region Properties
        int m_AnimationInterval;

        public int AnimationInterval
        {
            get { return m_AnimationInterval; }
            set
            {
                m_AnimationInterval = value;
            }
        }

        public BitmapSource BaseImage
        {
            get { return m_BaseImage; }
            set
            {
                m_BaseImage = value;
                if (m_BaseImage != null)
                {
                    if (staticImage)
                    {
                        Width = m_BaseImage.Width;
                        Height = m_BaseImage.Height;
                    }
                    else if (m_Columns > 0 && m_Rows > 0)
                    {
                        Width = m_BaseImage.Width / m_Columns;
                        Height = m_BaseImage.Height / m_Rows;
                    }
                }
                else
                {
                    Width = 0;
                    Height = 0;
                }
            }
        }

        public int Columns
        {
            get { return m_Columns; }
            set { m_Columns = value; }
        }

        public int Rows
        {
            get { return m_Rows; }
            set { m_Rows = value; }
        }

        public bool StaticImage
        {
            get { return staticImage; }
            set { staticImage = value; }
        }

        public bool CurrentlyAnimating
        {
            get
            {
                return animationStarted;
            }
        }

        public bool SkipFirstFrame
        {
            get { return m_SkipFirstFrame; }
            set { m_SkipFirstFrame = value; }
        }

        #endregion

        //Constructor
        public AnimationControl()
        {
            m_AnimationInterval = 30;
        }

        private TimeSpan LastRenderTime { get; set; }

        private int currentFrame;

        private void CompositionTarget_Rendering(object sender, System.EventArgs e)
        {
            TimeSpan timeSinceLastRender;

            // Enforce FramesPerSecond if BypassFramesPerSecond is false.
            timeSinceLastRender = (DateTime.Now.TimeOfDay - LastRenderTime);
            if (timeSinceLastRender.TotalSeconds * 1000 < m_AnimationInterval)
                return;

            LastRenderTime = DateTime.Now.TimeOfDay;

            Trace.Assert(!staticImage);
            if (frames != null && frames.Length > (SkipFirstFrame ? 1 : 0))
            {
                currentFrame++;
                if (frames.Length <= currentFrame)
                {
                    currentFrame = SkipFirstFrame ? 1 : 0;
                }
                Source = frames[currentFrame];
            }
        }

        bool animationStarted;
        ImageSource[] frames;

        //methods
        public void StartAnimation()
        {
            if (animationStarted) return;
            if (staticImage)
            {
                this.Source = BaseImage;
            }
            else
            {
                int k = 0;
                Width = m_Columns == 0 ? 0 : m_BaseImage.Width / m_Columns;
                Height = m_Rows == 0 ? 0 : m_BaseImage.Height / m_Rows;

                int frameWidth = (int)Width;
                int frameHeight = (int)Height;
                frames = new ImageSource[m_Rows * m_Columns];
                for (int rowOn = 0; rowOn < m_Rows; rowOn++)
                {
                    for (int columnOn = 0; columnOn < m_Columns; columnOn++)
                    {
                        frames[k++] = new CroppedBitmap(BaseImage,
                            new Int32Rect(columnOn * frameWidth, rowOn * frameHeight, frameWidth, frameHeight));
                    }
                }
                animationStarted = true;
                currentFrame = 0;
                LastRenderTime = DateTime.Now.TimeOfDay;
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }            
        }

        public void StopAnimation()
        {
            if (!animationStarted) return;
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            animationStarted = false;
        }
    }
}
