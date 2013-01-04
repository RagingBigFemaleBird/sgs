using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sanguosha.UI.Animations
{
    /// <summary>
    /// Adorner that disables all controls that fall under it
    /// </summary>
    public class FiredImage : UserControl
    {
        private BitmapPalette _pallette = null;
        private const int DPI = 96;
        private FireGenerator _fireGenerator;
        private object _fireGenLock;

        /// <summary>
        /// Constructor for the adorner
        /// </summary>
        /// <param name="adornerElement">The element to be adorned</param>
        public FiredImage()
        {
            _fireGenLock = new object();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += new EventHandler(CompositionTarget_Rendering);
            timer.Start();
        }

        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(BitmapSource), typeof(FiredImage), new UIPropertyMetadata(null, OnImageSourceChanged));

        protected static void OnImageSourceChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            FiredImage adorner = sender as FiredImage;
            if (sender != null)
            {
                adorner._CreateFireGenerator();
            }
        }

        private void _CreateFireGenerator()
        {
            lock (_fireGenLock)
            {
                BitmapSource image = Source;
                if (image == null)
                {
                    _fireGenerator = null;
                    return;
                }
                int imageSize = image.PixelWidth * image.PixelHeight;
                int stride = image.PixelWidth * 4;
                _fireGenerator = new FireGenerator(image.PixelWidth, image.PixelHeight);
                byte[] baseImage = new byte[stride * image.PixelHeight];
                image.CopyPixels(baseImage, stride, 0);
                for (int i = 0; i < imageSize; i++)
                {
                    _fireGenerator.BaseAlphaChannel[i] = baseImage[i * 4 + 3];
                }
            }
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        /// <summary>
        /// Called to draw on screen
        /// </summary>
        /// <param name="drawingContext">The drawind context in which we can draw</param>
        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            lock (_fireGenLock)
            {
                if (_fireGenerator == null)
                    return;

                // only set the pallette once (dont do in constructor as causes odd errors if exception occurs)
                if (_pallette == null)
                    _pallette = SetupFirePalette();

                _fireGenerator.FadeFactor = FadeFactor;
                _fireGenerator.UpdateFire();

                BitmapSource bs = BitmapSource.Create(_fireGenerator.Width, _fireGenerator.Height, DPI, DPI,
                    PixelFormats.Indexed8, _pallette, _fireGenerator.FireData, _fireGenerator.Width);
                drawingContext.DrawImage(bs, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            }
        }

        public LinearGradientBrush Palette
        {
            get { return (LinearGradientBrush)GetValue(PaletteProperty); }
            set { SetValue(PaletteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Palette.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaletteProperty =
            DependencyProperty.Register("Palette", typeof(LinearGradientBrush), typeof(FiredImage),
            new UIPropertyMetadata(null, new PropertyChangedCallback((o, c) => { (o as FiredImage).SetupFirePalette(); })));



        public int FadeFactor
        {
            get { return (int)GetValue(FadeFactorProperty); }
            set { SetValue(FadeFactorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FadeFactor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FadeFactorProperty =
            DependencyProperty.Register("FadeFactor", typeof(int), typeof(FiredImage), new UIPropertyMetadata(1));

        

        private BitmapPalette SetupFirePalette()
        {
            List<Color> myList = new List<Color>();
            // seutp the basic array we will modify
            for (int i = 0; i <= 255; i++)
            {
               myList.Add(new Color());
            }
        
            if (Palette == null)
            {           
                for (int i = 0; i < 64; i++)
                {
                    Color c1 = new Color();
                    c1.R = (byte)(i * 4);
                    c1.G = (byte)(0);
                    c1.B = (byte)(0);
                    c1.A = (byte)(i * 4);
                    myList[i] = c1;

                    Color c2 = new Color();
                    c2.R = (byte)(255);
                    c2.G = (byte)(i * 4);
                    c2.B = (byte)(0);
                    c2.A = 255;
                    myList[i + 64] = c2;

                    Color c3 = new Color();
                    c3.R = (byte)(255);
                    c3.G = (byte)(255);
                    c3.B = (byte)(i * 4);
                    c3.A = 255;
                    myList[i + 128] = c3;

                    Color c4 = new Color();
                    c4.R = (byte)(255);
                    c4.G = (byte)(255);
                    c4.B = (byte)(255);
                    c4.A = 255;
                    myList[i + 192] = c4;
                }
            }
            else
            {
                // seutp the basic array we will modify
                for (int i = 0; i <= 255; i++)
                {
                    double percentage = i / 255d;
                    int j;
                    for (j = 0; j < Palette.GradientStops.Count - 1; j++)
                    {
                        double offset1 = Palette.GradientStops[j].Offset;
                        double offset2 = Palette.GradientStops[j + 1].Offset;
                        
                        if (offset1 != offset2 && offset1 <= percentage &&
                            offset2 >= percentage)
                        {
                            Color color1 = Palette.GradientStops[j].Color;
                            Color color2 = Palette.GradientStops[j + 1].Color;
                            Color color = new Color();
                            color.R = (byte)((int)(color2.R - color1.R) / (offset2 - offset1) * (percentage - offset1) + color1.R); 
                            color.G = (byte)((int)(color2.G - color1.G) / (offset2 - offset1) * (percentage - offset1) + color1.G); 
                            color.B = (byte)((int)(color2.B - color1.B) / (offset2 - offset1) * (percentage - offset1) + color1.B); 
                            color.A = (byte)((int)(color2.A - color1.A) / (offset2 - offset1) * (percentage - offset1) + color1.A);
                            myList[i] = color;
                            break;
                        }
                    }
                }
            }


            BitmapPalette bp = new BitmapPalette(myList);
            return bp;
        }
    }
}
