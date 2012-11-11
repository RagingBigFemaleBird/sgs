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
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Diagnostics;

using Sanguosha.Core.Cards;

namespace Sanguosha.UI.Controls
{
    public class SuitColorToColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SuitColorType color = (SuitColorType)value;
            if (color == SuitColorType.Black)
            {
                return new SolidColorBrush(Colors.Black);
            }
            else if (color == SuitColorType.Red)
            {
                Color red = new Color();
                red.R = 212;
                red.A = 255;
                return new SolidColorBrush(red);
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for CardView.xaml
    /// </summary>
    public partial class CardView : UserControl
    {
        public CardView()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += CardView_MouseLeftButtonDown;
            this.IsEnabledChanged += CardView_IsEnabledChanged;
            this.MouseEnter += CardView_MouseEnter;
            this.MouseLeave += CardView_MouseLeave;
            _daMoveX = new DoubleAnimation();
            _daMoveY = new DoubleAnimation();            
            Storyboard.SetTarget(_daMoveX, this);
            Storyboard.SetTargetProperty(_daMoveX, new PropertyPath(Canvas.LeftProperty));
            Storyboard.SetTarget(_daMoveY, this);
            Storyboard.SetTargetProperty(_daMoveY, new PropertyPath(Canvas.TopProperty));                        
            _moveAnimation = new Storyboard();
            _moveAnimation.Children.Add(_daMoveX);
            _moveAnimation.Children.Add(_daMoveY);            
            _moveAnimation.AccelerationRatio = 0.2;
        }

        void CardView_MouseLeave(object sender, MouseEventArgs e)
        {
            (Resources["sbUnHighlight"] as Storyboard).Begin();
        }

        void CardView_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsEnabled)
            {
                (Resources["sbHighlight"] as Storyboard).Begin();
            }
        }

        void CardView_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                (Resources["sbUnHighlight"] as Storyboard).Begin();
            }
            else if (IsMouseOver)
            {
                (Resources["sbHighlight"] as Storyboard).Begin();
            }
        }

        void CardView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }        

        void CardView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CardViewModel model = DataContext as CardViewModel;
            if (model == null) return;
            model.IsSelected = !model.IsSelected;
        }       

        public Card Card
        {
            get
            {
                CardViewModel model = DataContext as CardViewModel;
                if (model == null) return null;
                return model.Card;
            }
        }

        public CardViewModel CardModel
        {
            get
            {
                return DataContext as CardViewModel;
            }
        }

        public static double WidthHeightRatio = 0.7154;

        DoubleAnimation _daMoveX;
        DoubleAnimation _daMoveY;        
        Storyboard _moveAnimation;

        public void Rebase(double transitionInSeconds)
        {
            if (Position == null) return;
            if (Parent == null || !(Parent is Canvas)) return;

            double destX = Position.X + Offset.X;
            double destY = Position.Y + Offset.Y;
            
            lock (_daMoveX)
            {
                double x = (double)GetValue(Canvas.LeftProperty);
                double y = (double)GetValue(Canvas.TopProperty);
                double opacity = (double)GetValue(Canvas.OpacityProperty);

                if (double.IsNaN(x) || double.IsNaN(y))
                {
                    SetValue(Canvas.LeftProperty, destX);
                    SetValue(Canvas.TopProperty, destY);
                    return;
                }


                Point point = new Point(x, y);
                _daMoveX.From = x;
                _daMoveY.From = y;                
                _daMoveX.To = destX;
                _daMoveY.To = destY;
                _daMoveX.Duration = TimeSpan.FromSeconds(transitionInSeconds);
                _daMoveY.Duration = TimeSpan.FromSeconds(transitionInSeconds);

                _moveAnimation.Duration = TimeSpan.FromSeconds(transitionInSeconds);
                _moveAnimation.Begin(this, true);
            }
        }

        public void Appear(double duration)
        {
            if (duration == 0) Opacity = 1.0;
            Storyboard appear = Resources["sbAppear"] as Storyboard;
            appear.SpeedRatio = 1 / duration;            
            appear.Begin();
        }

        public void Disappear(double duration)
        {
            Panel panel = this.Parent as Panel;
            if (panel == null) return;
            else if (duration == 0) Opacity = 0.0;
            Storyboard disappear = Resources["sbDisappear"] as Storyboard;
            disappear.Completed += new EventHandler((o, e2) =>
            {
                panel.Children.Remove(this);
            });
            disappear.SpeedRatio = 1 / duration;
            disappear.Begin();            
        }

        private static void OnOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CardView card = d as CardView;
            if (card == null) return;
            card.Rebase(0.2d);
        }

        #region Dependency Properties

        public Point Offset
        {
            get { return (Point)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Offset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(Point), typeof(CardView), new UIPropertyMetadata(new Point(0, 0),
                new PropertyChangedCallback(OnOffsetPropertyChanged)));
        
        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(CardView), new UIPropertyMetadata(new Point(0,0)));
        
        #endregion

        #region Card Creation/Destruction Helpers
        public static CardView CreateCard(Card card, int width = 93, int height = 130)
        {
            return new CardView()
            {
                DataContext = new CardViewModel() { Card = card },
                Width = width,
                Height = height,
                Opacity = 0d
            };
        }

        public static IList<CardView> CreateCards(IList<Card> cards)
        {
            List<CardView> cardViews = new List<CardView>();
            foreach (Card card in cards)
            {
                cardViews.Add(CreateCard(card));
            }
            return cardViews;
        }
        #endregion

        #region Helper Tags
        public int DiscardDeckClearTimeStamp
        {
            get;
            set;
        }
        #endregion
    }
}
