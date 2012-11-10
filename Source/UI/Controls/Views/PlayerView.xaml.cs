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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Sanguosha.Core.Cards;
using System.Diagnostics;
using Sanguosha.UI.Animations;
using Sanguosha.Core.Games;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for PlayerInfoView.xaml
    /// </summary>
    public partial class PlayerView : PlayerViewBase
    {
        public PlayerView()
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
            else if (e.PropertyName == "IsDead")
            {
                Storyboard deathAnimation = (Resources["sbDeathTransition"] as Storyboard);
                Trace.Assert(deathAnimation != null);
                if (model.IsDead)
                {
                    deathAnimation.Begin();
                }
                else
                {
                    deathAnimation.Stop();
                }
            }
            else if (e.PropertyName == "IsIronShackled")
            {
                if (model.IsIronShackled)
                {
                    tieSuoAnimation2.Start();
                }
            }
        }

        public override void PlayIronShackleAnimation()
        {
            tieSuoAnimation.Start();
        }

        private void mainArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PlayerViewModel model = DataContext as PlayerViewModel;
            model.IsSelected = !model.IsSelected;
        }
		
		
        private void btnSpectate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			EventHandler handler = OnRequestSpectate;
			if (handler != null)
			{
				handler(this, new EventArgs());
			}
        }
		
		public event EventHandler OnRequestSpectate;

        #region PlayerInfoViewBase Members

        protected override void AddDelayedTool(CardView card)
        {
            SmallDelayedToolView dtv = new SmallDelayedToolView() { Width = 23, Height = 24 };
            dtv.DataContext = card.CardModel;
            dtv.Opacity = 0;
            dtv.Margin = new Thickness(0, 0, 50d, 0);
            delayedToolsDock.Children.Add(dtv);

            Point dest = delayedToolsDock.TranslatePoint(new Point(-11.5, delayedToolsDock.ActualHeight / 2),
                                                                   ParentGameView.GlobalCanvas);
            dest.Offset(-card.Width / 2, -card.Height / 2);
            card.Position = dest;
            card.CardOpacity = 0.0d;
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
            Storyboard.SetTargetProperty(animation1, new PropertyPath(SmallDelayedToolView.MarginProperty));
            Storyboard.SetTargetProperty(animation2, new PropertyPath(SmallDelayedToolView.OpacityProperty));
            storyBoard.Children.Add(animation1);
            storyBoard.Children.Add(animation2);
            storyBoard.Begin();
        }

        protected override CardView RemoveDelayedTool(Card card)
        {
            SmallDelayedToolView dtv = null;
            foreach (var tmpDtv in delayedToolsDock.Children)
            {
                dtv = tmpDtv as SmallDelayedToolView;
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

        protected override void AddEquipment(CardView card)
        {
            Equipment equip = card.Card.Type as Equipment;

            if (equip == null)
            {
                throw new ArgumentException("Cannot add non-equip to equip area.");
            }

            SmallEquipView equipLabel = new SmallEquipView();
            equipLabel.DataContext = card.CardModel;
            
            Canvas targetArea = null;
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
            equipLabel.Opacity = 0;

            if (targetArea.Children.Count != 0)
            {
                throw new ArgumentException("Duplicate equip not allowed.");
            }
            targetArea.Children.Clear();
            targetArea.Children.Add(equipLabel);

            card.Position = ComputeCardCenterPos(card, targetArea);
            card.CardOpacity = 0.0;
            card.DisappearAfterMove = true;
            card.Rebase(0.3d);

            Storyboard storyBoard = new Storyboard();
            DoubleAnimation animation1 = new DoubleAnimation();
            DoubleAnimation animation2 = new DoubleAnimation();
            animation1.From = -50d;
            animation1.To = 0d;
            animation2.To = 1d;
            animation1.Duration = TimeSpan.FromMilliseconds(500);
            animation2.Duration = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTarget(animation1, equipLabel);
            Storyboard.SetTarget(animation2, equipLabel);
            Storyboard.SetTargetProperty(animation1, new PropertyPath(Canvas.TopProperty));
            Storyboard.SetTargetProperty(animation2, new PropertyPath(SmallEquipView.OpacityProperty));
            storyBoard.Children.Add(animation1);
            storyBoard.Children.Add(animation2);
            storyBoard.Begin();
        }

        protected override CardView RemoveEquipment(Card card)
        {
            Trace.Assert(card.Id >= 0, "Cannot remove unknown card from equip area.");
            Equipment equip = GameEngine.CardSet[card.Id].Type as Equipment;

            if (equip == null)
            {
                throw new ArgumentException("Cannot add non-equip to equip area.");
            }

            Canvas targetArea = null;
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

            SmallEquipView equipLabel = targetArea.Children[0] as SmallEquipView;
            targetArea.Children.Clear();

            CardView result = CardView.CreateCard(card);
            ParentGameView.GlobalCanvas.Children.Add(result);
            result.Opacity = 0;
            result.Position = ComputeCardCenterPos(result, targetArea);
            result.Rebase(0);

            Storyboard storyBoard = new Storyboard();
            DoubleAnimation animation1 = new DoubleAnimation();
            DoubleAnimation animation2 = new DoubleAnimation();
            animation1.To = 50d;
            animation2.To = 0.0d;
            animation1.Duration = TimeSpan.FromMilliseconds(500);
            animation2.Duration = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTarget(animation1, equipLabel);
            Storyboard.SetTarget(animation2, equipLabel);
            Storyboard.SetTargetProperty(animation1, new PropertyPath(Canvas.TopProperty));
            Storyboard.SetTargetProperty(animation2, new PropertyPath(SmallEquipView.OpacityProperty));
            storyBoard.Children.Add(animation1);
            storyBoard.Children.Add(animation2);
            storyBoard.Begin();
            return result;
        }

        protected override void AddRoleCard(CardView card)
        {
            card.Position = ComputeCardCenterPos(card, cbRoleBox);
            
            ScaleTransform scale = new ScaleTransform();
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scale);
            card.RenderTransform = transformGroup;

            card.RenderTransformOrigin = new Point(0.5, 0.5);
            DoubleAnimation scaleXAnim = new DoubleAnimation(0.2, new Duration(TimeSpan.FromSeconds(0.5d)));
            DoubleAnimation scaleYAnim = new DoubleAnimation(0.2, new Duration(TimeSpan.FromSeconds(0.5d)));
            Storyboard.SetTarget(scaleXAnim, card);
            Storyboard.SetTarget(scaleYAnim, card);
            Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(scaleXAnim);
            storyboard.Children.Add(scaleYAnim);
            storyboard.AccelerationRatio = 0.4d;
            storyboard.Begin();

            card.CardOpacity = 1.0;
            card.DisappearAfterMove = true;
            card.Rebase(0.5d);
        }

        protected override CardView RemoveRoleCard(Card card)
        {
            CardView result = CardView.CreateCard(card);
            ParentGameView.GlobalCanvas.Children.Add(result);
            result.Opacity = 0;
            result.Position = ComputeCardCenterPos(result, cbRoleBox);
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

        public override void Tremble()
        {
            (Resources["sbTremble"] as Storyboard).Begin();
        }
        #endregion // PlayerInfoViewBase Members

    }
}
