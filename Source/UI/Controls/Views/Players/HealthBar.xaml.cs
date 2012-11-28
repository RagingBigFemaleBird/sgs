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
    /// <summary>
    /// Interaction logic for HealthBar.xaml
    /// </summary>
    public partial class HealthBar : UserControl
    {
        public HealthBar()
        {
            InitializeComponent();
        }

        #region Re-layout Functions

        private static void OnHealthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HealthBar bar = (HealthBar)d;
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
                for (int i = Math.Max(newHealth, 0); i < oldHealth; i++)
                {
                    Trace.Assert(i < 5);
                    LoseHealthAnimation animation = new LoseHealthAnimation();
                    AlignLoseHealthAnimation(animation, wpSmallHealth.Children[i] as Image);
                    animations.Add(animation);
                }            
            }

            foreach (var animation in animations)
            {
                animation.Completed += new EventHandler(animation_Completed);
                canvasRoot.Children.Add(animation);
                animation.Start();
            }
        }

        void animation_Completed(object sender, EventArgs e)
        {
            canvasRoot.Children.Remove(sender as UIElement);
        }
        

        private void AlignLoseHealthAnimation(LoseHealthAnimation animation,Image bloodDrop)
        {
            UpdateLayout();
            Point leftBottom = bloodDrop.TranslatePoint(new Point(0, bloodDrop.ActualHeight), canvasRoot);
            animation.Width = 50;
            animation.Height = 120;
            animation.SetValue(Canvas.LeftProperty, leftBottom.X - 8);
            animation.SetValue(Canvas.BottomProperty, leftBottom.Y - 18);            
        }

        static void OnMaxHealthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HealthBar bar = (HealthBar)d;
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
            ImageSource source = Resources[string.Format("HealthBar.Digit.Small.{0}.{1}", strColor, strDigit)] as ImageSource;
            Image image = new Image(){Source = source};
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
            ImageSource image = Resources[string.Format("HealthBar.{0}.Small", healthRange)] as ImageSource;
            if (maxHealth > 5)
            {
                wpLargeHealth.Visibility = Visibility.Visible;
                wpSmallHealth.Visibility = Visibility.Hidden;
                imgBloodDrop.Source = image;
                wpLargeHealth.Children.Clear();

                int quotient = maxHealth;
                do
                {
                    int digit = quotient % 10;
                    quotient /= 10;
                    wpLargeHealth.Children.Insert(0, DigitToImage(digit, healthRange));
                } while (quotient > 0);
                wpLargeHealth.Children.Insert(0, DigitToImage(-1, healthRange));
                quotient = health;
                do
                {
                    int digit = quotient % 10;
                    quotient /= 10;
                    wpLargeHealth.Children.Insert(0, DigitToImage(digit, healthRange));
                } while (quotient > 0);                               
                wpLargeHealth.Children.Insert(0, imgBloodDrop);
            }
            else
            {
                wpLargeHealth.Visibility = Visibility.Hidden;
                wpSmallHealth.Visibility = Visibility.Visible;
                wpSmallHealth.Children.Clear();                
                int i = 0;
                for (i = 0; i < health; i++)
                {
                    Image bloodDrop = new Image() { Source = image, Height = this.Height };                   
                    wpSmallHealth.Children.Add(bloodDrop);
                }
                image = Resources["HealthBar.0.Small"] as ImageSource;
                for (; i < maxHealth; i++)
                {
                    Image bloodDrop = new Image() { Source = image, Height = this.Height };
                    wpSmallHealth.Children.Add(bloodDrop);
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
            DependencyProperty.Register("Health", typeof(int), typeof(HealthBar), new UIPropertyMetadata(
                                        new PropertyChangedCallback(OnHealthChanged)));

        public int MaxHealth
        {
            get { return (int)GetValue(MaxHealthProperty); }
            set { SetValue(MaxHealthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxHealth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxHealthProperty =
            DependencyProperty.Register("MaxHealth", typeof(int), typeof(HealthBar), new UIPropertyMetadata(
                                        new PropertyChangedCallback(OnMaxHealthChanged)));
        
        #endregion
    }
}
