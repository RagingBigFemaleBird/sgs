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
using Sanguosha.Core.Games;

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
            handCardArea.OnHandCardMoved += handCardArea_OnHandCardMoved;
        }

        void handCardArea_OnHandCardMoved(int oldPlace, int newPlace)
        {
            Game.CurrentGame.MoveHandCard(PlayerModel.Player, oldPlace, newPlace);
        }

        public override GameView ParentGameView
        {
            get
            {
                return base.ParentGameView;
            }
            set
            {
                base.ParentGameView = value;
                handCardArea.ParentCanvas = value.GlobalCanvas;
            }
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

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PlayerViewModel model = DataContext as PlayerViewModel;
            model.IsSelected = !model.IsSelected;
        }

        protected override void AddHandCards(IList<CardView> cards)
        {
            foreach (var card in cards)
            {
                card.DragDirection = DragDirection.Horizontal;
            }
            handCardArea.AddCards(cards, 0.5d);
        }

        protected override IList<CardView> RemoveHandCards(IList<Card> cards)
        {
            var cardsToRemove = new List<CardView>();
            foreach (var card in cards)
            {
                bool found = false;
                foreach (var cardView in handCardArea.Cards)
                {
                    CardViewModel viewModel = cardView.DataContext as CardViewModel;
                    Trace.Assert(viewModel != null);
                    if (viewModel.Card == card)
                    {
                        cardsToRemove.Add(cardView);
                        cardView.DragDirection = DragDirection.None;
                        PlayerModel.HandCards.Remove(viewModel);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    cardsToRemove.Add(CardView.CreateCard(card));
                }
            }
            Trace.Assert(cardsToRemove.Count == cards.Count);
            handCardArea.RemoveCards(cardsToRemove);
            return cardsToRemove;
        }

        public override void UpdateCardAreas()
        {
            handCardArea.RearrangeCards(0d);
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
            card.Disappear(0.5d);            
            card.Rebase(0.5d);

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
            Trace.Assert(card.Id >= 0, "Cannot remove unknown card from equip area.");
            Equipment equip = GameEngine.CardSet[card.Id].Type as Equipment;

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
            Point dest = targetArea.TranslatePoint(new Point(targetArea.Width / 2, targetArea.Height / 2),
                                                   ParentGameView.GlobalCanvas);
            dest.Offset(-result.Width / 2, -result.Height / 2);
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
            card.Disappear(0.5d);
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

        protected override void AddRoleCard(CardView card)
        {
            card.Position = ComputeCardCenterPos(card, cbRoleBox);
            card.RenderTransformOrigin = new Point(0.5, 0.5);
            card.Opacity = 1.0;

            RotateTransform rotate = new RotateTransform();
            ScaleTransform scale = new ScaleTransform();
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(rotate);
            transformGroup.Children.Add(scale);
            card.RenderTransform = transformGroup;

            DoubleAnimation rotateAnim = new DoubleAnimation(720, new Duration(TimeSpan.FromSeconds(0.8d)));
            DoubleAnimation scaleXAnim = new DoubleAnimation(0.25, new Duration(TimeSpan.FromSeconds(0.8d)));
            DoubleAnimation scaleYAnim = new DoubleAnimation(0.25, new Duration(TimeSpan.FromSeconds(0.8d)));
            Storyboard.SetTarget(rotateAnim, card);
            Storyboard.SetTarget(scaleXAnim, card);
            Storyboard.SetTarget(scaleYAnim, card);
            Storyboard.SetTargetProperty(rotateAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)"));
            Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(ScaleTransform.ScaleY)"));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(rotateAnim);
            storyboard.Children.Add(scaleXAnim);
            storyboard.Children.Add(scaleYAnim);
            storyboard.AccelerationRatio = 0.4d;
            storyboard.Begin();

            card.Disappear(1.2d);
            card.Rebase(1.2d);
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
    }
}
