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
using Sanguosha.Core.Cards;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using Sanguosha.UI.Animations;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for MainPlayerInfoView.xaml
    /// </summary>
    public partial class MainPlayerView : PlayerViewBase
    {
        public MainPlayerView()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(PlayerInfoView_DataContextChanged);
            _OnPropertyChanged = new PropertyChangedEventHandler(model_PropertyChanged);
            HandCardArea = handCardPlaceHolder;
        }

        private PropertyChangedEventHandler _OnPropertyChanged;

        void PlayerInfoView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PlayerViewModel model = e.OldValue as PlayerViewModel;
            if (model != null)
            {
                model.PropertyChanged -= _OnPropertyChanged;
            }
            model = e.NewValue as PlayerViewModel;
            if (model != null)
            {
                model.PropertyChanged += _OnPropertyChanged;
                cbRoleBox.DataContext = model.PossibleRoles;
            }
        }       

        void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PlayerViewModel model = sender as PlayerViewModel;
            if (e.PropertyName == "CurrentPhase")
            {
                if (model.CurrentPhase == Core.Games.TurnPhase.Inactive)
                {
                    imgPhase.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    imgPhase.Visibility = System.Windows.Visibility.Visible;
                    Storyboard animation = Resources["sbPhaseChange"] as Storyboard;
                    animation.Begin(this);
                }
            }
            else if (e.PropertyName == "TimeOutSeconds")
            {
                Duration duration = new Duration(TimeSpan.FromSeconds(model.TimeOutSeconds));         
                DoubleAnimation doubleanimation = new DoubleAnimation(100d, 0d, duration);
                progressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);                
            }
        }       

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PlayerViewModel model = DataContext as PlayerViewModel;
            model.IsSelected = !model.IsSelected;
        }

        protected override void AddEquipment(CardView card)
        {
            Equipment equip = card.Card.Type as Equipment;

            if (equip == null)
            {
                throw new ArgumentException("Cannot add non-equip to equip area.");
            }

            ToggleButton button = new ToggleButton();
            button.Style = Resources["BigEquipToggleButton"] as Style;
            button.HorizontalAlignment = HorizontalAlignment.Left;
            button.VerticalAlignment = VerticalAlignment.Top;

            Grid targetArea = null;
            switch (equip.Category)
            {
                case CardCategory.Weapon:
                    button.DataContext = PlayerModel.WeaponCommand;
                    targetArea = weaponArea;
                    break;
                case CardCategory.Armor:
                    button.DataContext = PlayerModel.ArmorCommand;
                    targetArea = armorArea;
                    break;
                case CardCategory.DefensiveHorse:
                    button.DataContext = PlayerModel.DefensiveHorseCommand;
                    targetArea = horse1Area;
                    break;
                case CardCategory.OffensiveHorse:
                    button.DataContext = PlayerModel.OffensiveHorseCommand;
                    targetArea = horse2Area;
                    break;
                default:
                    throw new ArgumentException("Cannot install non-equips to equip area.");
            }

            button.Width = targetArea.Width;
            button.Height = targetArea.Height;
            button.Opacity = 0;

            if (targetArea.Children.Count != 0)
            {
                throw new ArgumentException("Duplicate equip not allowed.");
            }
            targetArea.Children.Clear();
            targetArea.Children.Add(button);

            Point dest = targetArea.TranslatePoint(new Point(targetArea.Width / 2, targetArea.Height / 2),
                                                   ParentGameView.GlobalCanvas);
            dest.Offset(-card.Width / 2, -card.Height / 2);
            card.Position = dest;
            card.CardOpacity = 1.0;
            card.DisappearAfterMove = true;
            card.Rebase(0.3d);

            Storyboard storyBoard = new Storyboard();
            ThicknessAnimation animation1 = new ThicknessAnimation();
            DoubleAnimation animation2 = new DoubleAnimation();
            animation1.From = new Thickness(-100d, 0d, 0d, 0d);
            animation1.To = new Thickness(0d, 0d, 0d, 0d);
            animation2.To = 1.0d;
            animation1.Duration = TimeSpan.FromMilliseconds(500);
            animation2.Duration = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTarget(animation1, button);
            Storyboard.SetTarget(animation2, button);
            Storyboard.SetTargetProperty(animation1, new PropertyPath(ToggleButton.MarginProperty));
            Storyboard.SetTargetProperty(animation2, new PropertyPath(ToggleButton.OpacityProperty));
            storyBoard.Children.Add(animation1);
            storyBoard.Children.Add(animation2);
            storyBoard.Begin();
        }

        protected override CardView RemoveEquipment(Card card)
        {
            Equipment equip = card.Type as Equipment;

            if (equip == null)
            {
                throw new ArgumentException("Cannot add non-equip to equip area.");
            }            

            Grid targetArea = null;
            switch (equip.Category)
            {
                case CardCategory.Weapon:
                    targetArea = weaponArea;
                    break;
                case CardCategory.Armor:
                    targetArea = armorArea;
                    break;
                case CardCategory.DefensiveHorse:
                    targetArea = horse1Area;
                    break;
                case CardCategory.OffensiveHorse:
                    targetArea = horse2Area;
                    break;
                default:
                    throw new ArgumentException("Cannot install non-equips to equip area.");
            }
            
            if (targetArea.Children.Count == 0)
            {
                throw new ArgumentException("No equip is found.");
            }
            ToggleButton button = targetArea.Children[0] as ToggleButton;
            targetArea.Children.Clear();
            
            CardView result = CardView.CreateCard(card);
            ParentGameView.GlobalCanvas.Children.Add(result);
            result.Opacity = 0;
            Point dest = targetArea.TranslatePoint(new Point(0, 0), ParentGameView.GlobalCanvas);
            result.Position = dest;
            result.Rebase(0);

            Storyboard storyBoard = new Storyboard();
            ThicknessAnimation animation1 = new ThicknessAnimation();
            DoubleAnimation animation2 = new DoubleAnimation();
            animation1.To = new Thickness(100d, 30d, 0d, 0d);
            animation2.To = 0.0d;
            animation1.Duration = TimeSpan.FromMilliseconds(500);
            animation2.Duration = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTarget(animation1, button);
            Storyboard.SetTarget(animation2, button);
            Storyboard.SetTargetProperty(animation1, new PropertyPath(ToggleButton.MarginProperty));
            Storyboard.SetTargetProperty(animation2, new PropertyPath(ToggleButton.OpacityProperty));
            storyBoard.Children.Add(animation1);
            storyBoard.Children.Add(animation2);
            storyBoard.Begin();
            return result;
        }

        protected override void AddDelayedTool(CardView card)
        {
            LargeDelayedToolView dtv = new LargeDelayedToolView() { Width=30, Height=30 };
            dtv.DataContext = card.CardModel;
            dtv.Opacity = 0;
            dtv.Margin = new Thickness(50d, 0, 0, 0);
            delayedToolsDock.Children.Add(dtv);
            dtv.Opacity = 1d;
            dtv.Margin = new Thickness(0d, 0, 0, 0);


            Point dest = delayedToolsDock.TranslatePoint(new Point(delayedToolsDock.ActualWidth + 15, delayedToolsDock.ActualHeight / 2),
                                                                   ParentGameView.GlobalCanvas);
            dest.Offset(-card.Width / 2, -card.Height / 2);
            card.Position = dest;
            card.CardOpacity = 1.0d;
            card.DisappearAfterMove = true;
            card.Rebase(0.5d);
            
            Storyboard storyBoard = new Storyboard();
            ThicknessAnimation animation1 = new ThicknessAnimation();
            DoubleAnimation animation2 = new DoubleAnimation();
            animation1.To = new Thickness(0d, 0d, 0d, 0d);
            animation2.To = 1.0d;
            animation1.Duration = TimeSpan.FromMilliseconds(500);
            animation2.Duration = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTarget(animation1, dtv);
            Storyboard.SetTarget(animation2, dtv);
            Storyboard.SetTargetProperty(animation1, new PropertyPath(LargeDelayedToolView.MarginProperty));
            Storyboard.SetTargetProperty(animation2, new PropertyPath(LargeDelayedToolView.OpacityProperty));
            storyBoard.Children.Add(animation1);
            storyBoard.Children.Add(animation2);
            storyBoard.Begin();        
        }

        protected override CardView RemoveDelayedTool(Card card)
        {
            LargeDelayedToolView dtv = null;
            foreach (var tmpDtv in delayedToolsDock.Children)
            {
                dtv = tmpDtv as LargeDelayedToolView;
                Trace.Assert(dtv != null);
                CardViewModel model = dtv.DataContext as CardViewModel;
                Trace.Assert(model != null);
                if (model.Card == card) break;
                dtv = null;
            }

            Trace.Assert(dtv != null);

            Point dest = dtv.TranslatePoint(new Point(0, 0),
                                             ParentGameView.GlobalCanvas);
            delayedToolsDock.Children.Remove(dtv);
            CardView result = CardView.CreateCard(card);
            ParentGameView.GlobalCanvas.Children.Add(result);
            result.Opacity = 0;
            result.Position = dest;
            result.Rebase(0);

            return result;
        }

        public override void PlayAnimation(AnimationBase animation, int playCenter, Point offset)
        {
            Canvas canvas;
            if (playCenter == 1) canvas = animationCenter1;
            else canvas = animationCenter2;

            animation.SetValue(Canvas.LeftProperty, -animation.Width / 2 + offset.X);
            animation.SetValue(Canvas.TopProperty, -animation.Height / 2 + offset.Y);
            canvas.Children.Add(animation);
            animation.Start();
        }
    }
}
