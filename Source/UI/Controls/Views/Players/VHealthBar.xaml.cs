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
using System.Diagnostics;

using Sanguosha.UI.Animations;

namespace Sanguosha.UI.Controls
{
    // @todo: make this class a style of HealthBar
    /// <summary>
    /// Interaction logic for HealthBar.xaml
    /// </summary>    
    public partial class VHealthBar : UserControl
    {
        public VHealthBar()
        {
            InitializeComponent();
        }

        #region Re-layout Functions

        private static void OnHealthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VHealthBar bar = (VHealthBar)d;
            int oldHealth = (int)e.OldValue;
            int newHealth = (int)e.NewValue;            
            bar.Repaint();

            if (newHealth < oldHealth)
            {
                bar.PlayLoseHealthAnimation(oldHealth, newHealth);
            }
        }

        private void PlayLoseHealthAnimation(int oldHealth, int newHealth)
        {
            List<LoseHealthAnimation> animations = new List<LoseHealthAnimation>();

            if (MaxHealth > 5)
            {
                LoseHealthAnimation animation = new LoseHealthAnimation();
                animation.HorizontalAlignment = HorizontalAlignment.Left;
                animation.VerticalAlignment = VerticalAlignment.Bottom;
                AlignLoseHealthAnimation(animation, imgBloodDrop);
                animations.Add(animation);
            }
            else if (oldHealth <= MaxHealth && newHealth <= MaxHealth)
            {
                oldHealth = Math.Min(oldHealth, 5);
                newHealth = Math.Max(newHealth, 0);
                for (int i = newHealth; i < oldHealth; i++)
                {
                    Trace.Assert(i < 5);
                    LoseHealthAnimation animation = new LoseHealthAnimation();
                    AlignLoseHealthAnimation(animation, spSmallHealth.Children[i] as Image);
                    animations.Add(animation);                    
                }            
            }

            foreach (var animation in animations)
            {
                animation.Completed += animation_Completed;
                canvasRoot.Children.Add(animation);
                animation.Start();
            }
        }

        void animation_Completed(object sender, EventArgs e)
        {
            var anim = sender as FrameBasedAnimation;
            anim.Stop();
            canvasRoot.Children.Remove(anim);
        }
        

        private void AlignLoseHealthAnimation(LoseHealthAnimation animation, Image bloodDrop)
        {
            UpdateLayout();
            Point leftTop = bloodDrop.TranslatePoint(new Point(0, 0), canvasRoot);
            animation.Width = 50;
            animation.Height = 120;
            animation.SetValue(Canvas.LeftProperty, leftTop.X - 10);
            animation.SetValue(Canvas.TopProperty, leftTop.Y -100);            
        }

        static void OnMaxHealthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VHealthBar bar = (VHealthBar)d;
            bar.Repaint();
        }


        private Image DigitToImage(int digit, int healthRange)
        {
            Trace.Assert(healthRange <= 3);
            if (healthRange == 0) healthRange = 1;
            Trace.Assert(digit >= -1 && digit < 10);
            string strDigit = "Slash";
            if (digit != -1)
            {
                strDigit = digit.ToString();
            }
            string strColor = "Red";
            if (healthRange == 2) strColor = "Yellow";
            else if (healthRange == 3) strColor = "Green";
            ImageSource source = Resources[string.Format("HealthBar.Digit.Large.{0}.{1}", strColor, strDigit)] as ImageSource;
            Image image = new Image() { Source = source };
            return image;
        }

        private void Repaint()
        {
            int health = Health;
            int maxHealth = MaxHealth;

            int healthRange;
            if (health == 0)
            {
                healthRange = 0;
            }
            else if (health * 4 <= maxHealth || health == 1)
            {
                healthRange = 1;
            }
            else if (health * 2 <= maxHealth)
            {
                healthRange = 2;
            }
            else
            {
                healthRange = 3;
            }
            ImageSource image = Resources[string.Format("HealthBar.{0}.Large", healthRange)] as ImageSource;
            if (maxHealth > 5)
            {
                spLargeHealth.Visibility = Visibility.Visible;
                spSmallHealth.Visibility = Visibility.Hidden;
                imgBloodDrop.Source = image;
                spLargeHealth.Children.Clear();
                Thickness margin = new Thickness(0, 8, 0, 3);
                int quotient = maxHealth;
                StackPanel spMaxHealthText = new StackPanel();
                spMaxHealthText.Orientation = Orientation.Horizontal;
                do
                {
                    int digit = quotient % 10;
                    quotient /= 10;
                    spMaxHealthText.Children.Insert(0, DigitToImage(digit, healthRange));
                } while (quotient > 0);
                spMaxHealthText.Margin = margin;
                spMaxHealthText.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                spLargeHealth.Children.Insert(0, spMaxHealthText);
                Image slashImage = DigitToImage(-1, healthRange);
                slashImage.Margin = margin;
                slashImage.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                spLargeHealth.Children.Insert(0, slashImage);
                quotient = health;
                StackPanel spHealthText = new StackPanel();
                spHealthText.Orientation = Orientation.Horizontal;
                do
                {
                    int digit = quotient % 10;
                    quotient /= 10;
                    spHealthText.Children.Insert(0, DigitToImage(digit, healthRange));
                } while (quotient > 0);
                spHealthText.Margin = margin;
                spHealthText.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                spLargeHealth.Children.Insert(0, spHealthText);
                imgBloodDrop.Margin = margin;
                imgBloodDrop.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                spLargeHealth.Children.Insert(0, imgBloodDrop);                
            }
            else
            {
                spLargeHealth.Visibility = Visibility.Hidden;
                spSmallHealth.Visibility = Visibility.Visible;
                spSmallHealth.Children.Clear();
                int i = 0;
                for (i = 0; i < health; i++)
                {
                    Image bloodDrop = new Image() { Source = image };
                    bloodDrop.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    spSmallHealth.Children.Add(bloodDrop);
                }
                image = Resources["HealthBar.0.Large"] as ImageSource;
                for (; i < maxHealth; i++)
                {
                    Image bloodDrop = new Image() { Source = image };
                    spSmallHealth.Children.Add(bloodDrop);
                }                
            }
        }

        #endregion

        #region Dependency Properties
        public int Health
        {
            get { return (int)GetValue(HealthProperty); }
            set { SetValue(HealthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Health.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HealthProperty =
            DependencyProperty.Register("Health", typeof(int), typeof(VHealthBar), new UIPropertyMetadata(
                                        new PropertyChangedCallback(OnHealthChanged)));

        public int MaxHealth
        {
            get { return (int)GetValue(MaxHealthProperty); }
            set { SetValue(MaxHealthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxHealth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxHealthProperty =
            DependencyProperty.Register("MaxHealth", typeof(int), typeof(VHealthBar), new UIPropertyMetadata(
                                        new PropertyChangedCallback(OnMaxHealthChanged)));
        
        #endregion
    }
}
