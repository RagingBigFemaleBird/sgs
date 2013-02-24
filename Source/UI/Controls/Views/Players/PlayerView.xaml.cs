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
            Unloaded += PlayerView_Unloaded;
        }

        void PlayerView_Unloaded(object sender, RoutedEventArgs e)
        {
            grid.Effect = null;
            heroPhoto.Effect = null;
            heroPhoto2.Effect = null;
            this.DataContext = null;
        }

        public static void PlayerView_FlowDirectionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PlayerView view = sender as PlayerView;
            if (view != null)
            {
                view.LayoutRoot.FlowDirection = view.FlowDirection;
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
                Uri uri = GameSoundLocator.GetSystemSound("IronShackled");
                GameSoundPlayer.PlaySoundEffect(uri);
            }
        }

        public override void UpdateImpersonateStatus(bool isPrimaryHero)
        {
            Sanguosha.UI.Resources.FileNameToImageSourceConverter converter = new UI.Resources.FileNameToImageSourceConverter();

            var hero = isPrimaryHero ? PlayerModel.Hero1Model : PlayerModel.Hero2Model;

            Trace.Assert(hero != null);

            Storyboard sb;
            if (PlayerModel.Hero2 == null)
            {
                sb = (Resources["sbStartImpersonate"] as Storyboard);
            }
            else if (isPrimaryHero)
            {
                sb = (Resources["sbStartImpersonate1"] as Storyboard);
            }
            else
            {
                sb = (Resources["sbStartImpersonate2"] as Storyboard);
            }
            if (!string.IsNullOrEmpty(hero.ImpersonatedHeroName))
            {
                converter.StringFormat = "Resources/Images/Heroes/Full/{0}.png";
                converter.ResourceKeyFormat = "Hero.{0}.Image";
                Effects.RippleTransitionEffect effect;
                if (PlayerModel.Hero2 == null)
                {
                    converter.CropRect = new Int32Rect(28, 46, 220, 132);
                    effect = impersonateEffect;
                }
                else if (isPrimaryHero)
                {
                    converter.CropRect = new Int32Rect(60, 30, 208, 125);
                    effect = impersonateEffect1;
                }
                else
                {
                    converter.CropRect = new Int32Rect(63, 20, 126, 178);
                    effect = impersonateEffect2;
                }

                ImageSource source = converter.Convert(new object[] { this, hero.ImpersonatedHeroName }, typeof(ImageSource), null, null) as ImageSource;
                effect.Texture2 = new ImageBrush(source);
                sb.Begin();
            }
            else
            {
                sb.Stop();
            }
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
                privateCardArea.ParentCanvas = value.GlobalCanvas;
            }
        }

        public override void PlayIronShackleAnimation()
        {
            tieSuoAnimation.Start();
        }

        private void mainArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PlayerViewModel model = DataContext as PlayerViewModel;
            model.SelectOnce();
        }

        private void mainArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PlayerViewModel model = DataContext as PlayerViewModel;
            model.IsSelected = false;
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

        internal override void UpdateCards()
        {
            weaponArea.Children.Clear();
            armorArea.Children.Clear();
            horse1Area.Children.Clear();
            horse2Area.Children.Clear();
            delayedToolsDock.Children.Clear();

            base.UpdateCards();

            if (PlayerModel == null) return;
            var player = PlayerModel.Player;
            if (player == null) return;

            EquipCommand[] commands = { PlayerModel.WeaponCommand, PlayerModel.ArmorCommand,
                                        PlayerModel.DefensiveHorseCommand, PlayerModel.OffensiveHorseCommand };
            foreach (var equip in commands)
            {
                if (equip != null)
                {
                    AddEquipment(CardView.CreateCard(equip, ParentGameView.GlobalCanvas), true);
                }
            }

            foreach (var dt in player.DelayedTools())
            {
                AddDelayedTool(CardView.CreateCard(dt, ParentGameView.GlobalCanvas), true);
            }
        }

        protected override void AddHandCards(IList<CardView> cards, bool isFaked)
        {
            if (isFaked)
            {
                foreach (var card in cards)
                {
                    card.Disappear(0d);
                }
            }
            else
            {
                foreach (var card in cards)
                {
                    card.Disappear(1.0d, true);
                }
                handCardArea.RearrangeCards(cards);
            }
        }

        protected override void AddPrivateCards(IList<CardView> cards, bool isFaked)
        {
            if (isFaked)
            {
                foreach (var card in cards)
                {
                    card.Disappear(0d);
                }
            }
            else
            {
                privateCardArea.AddCards(cards);
            }
        }

        protected override IEnumerable<CardView> RemovePrivateCards(IList<Card> cards)
        {
            var cardsToRemove = new List<CardView>();
            foreach (var card in cards)
            {
                cardsToRemove.Add(CardView.CreateCard(card));
            }
            Trace.Assert(cardsToRemove.Count == cards.Count);
            privateCardArea.RemoveCards(cardsToRemove);
            return cardsToRemove;
        }

        protected override IList<CardView> RemoveHandCards(IList<Card> cards, bool isCopy)
        {
            var cardsToRemove = new List<CardView>();
            foreach (var card in cards)
            {
                cardsToRemove.Add(CardView.CreateCard(card));
            }
            Trace.Assert(cardsToRemove.Count == cards.Count);
            handCardArea.RemoveCards(cardsToRemove);
            return cardsToRemove;
        }

        public override void UpdateCardAreas()
        {
            handCardArea.RearrangeCards();
        }

        protected override void AddDelayedTool(CardView card, bool isFaked)
        {
            SmallDelayedToolView dtv = new SmallDelayedToolView() { Width = 23, Height = 24 };
            dtv.DataContext = card.CardModel;
            dtv.Opacity = 0;
            dtv.Margin = new Thickness(0, 0, 50d, 0);
            delayedToolsDock.Children.Add(dtv);

            if (isFaked)
            {
                card.Disappear(0d, true);
            }
            else
            {
                Point dest = delayedToolsDock.TranslatePoint(new Point(-11.5, delayedToolsDock.ActualHeight / 2),
                                                                       ParentGameView.GlobalCanvas);
                dest.Offset(-card.Width / 2, -card.Height / 2);
                card.Position = dest;
                card.Disappear(0.5d, true);
                card.Rebase();
            }

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

        protected override CardView RemoveDelayedTool(Card card, bool isCopy)
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
            if (!isCopy) delayedToolsDock.Children.Remove(dtv);

            CardView result = CardView.CreateCard(card);
            ParentGameView.GlobalCanvas.Children.Add(result);
            result.Opacity = 0;
            result.SetCurrentPosition(dest);

            return result;
        }

        protected override void AddEquipment(CardView card, bool isFaked)
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

            if (isFaked)
            {
                card.Disappear(0d, true);
            }
            else
            {
                card.Position = ComputeCardCenterPos(card, targetArea);
                card.Disappear(0.3d, true);
                card.Rebase();
            }

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

        protected override CardView RemoveEquipment(Card card, bool isCopy)
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
            if (!isCopy) targetArea.Children.Clear();

            CardView result = CardView.CreateCard(card);
            ParentGameView.GlobalCanvas.Children.Add(result);
            result.Opacity = 0;
            result.SetCurrentPosition(ComputeCardCenterPos(result, targetArea));
            return result;
        }

        protected override void AddRoleCard(CardView card, bool isFaked)
        {
            if (isFaked)
            {
                card.Disappear(0d, true);
                return;
            }

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
            card.AddRebaseAnimation(storyboard, 0.5d);
            storyboard.AccelerationRatio = 0.4d;
            storyboard.Begin();
            card.Disappear(0.5d, true);
        }

        protected override CardView RemoveRoleCard(Card card)
        {
            CardView result = CardView.CreateCard(card);
            ParentGameView.GlobalCanvas.Children.Add(result);
            result.Opacity = 0;
            result.SetCurrentPosition(ComputeCardCenterPos(result, cbRoleBox));
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
