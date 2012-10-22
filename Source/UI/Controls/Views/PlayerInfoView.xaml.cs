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

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for PlayerInfoView.xaml
    /// </summary>
    public partial class PlayerInfoView : PlayerInfoViewBase
    {
        public PlayerInfoView()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(PlayerInfoView_DataContextChanged);
            _OnPropertyChanged = new PropertyChangedEventHandler(model_PropertyChanged);
            HandCardArea = handCardPlaceHolder;
        }

        private PropertyChangedEventHandler _OnPropertyChanged;

        void PlayerInfoView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PlayerInfoViewModel model = e.OldValue as PlayerInfoViewModel;
            if (model != null)
            {
                model.PropertyChanged -= _OnPropertyChanged;
            }
            model = e.NewValue as PlayerInfoViewModel;
            if (model != null)
            {
                model.PropertyChanged += _OnPropertyChanged;
                cbRoleBox.DataContext = model.PossibleRoles;
            }
        }       

        void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PlayerInfoViewModel model = sender as PlayerInfoViewModel;
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
            else if (e.PropertyName == "PossibleRoles")
            {
                cbRoleBox.DataContext = model.PossibleRoles;
            }
        }

        private void mainArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PlayerInfoViewModel model = DataContext as PlayerInfoViewModel;
            model.IsSelected = !model.IsSelected;
        }

        #region PlayerInfoViewBase Members

        protected override void AddDelayedTool(CardView card)
        {
            LargeDelayedToolView dtv = new LargeDelayedToolView() { Width = 23, Height = 24 };
            dtv.DataContext = card.CardViewModel;
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
            Storyboard.SetTargetProperty(animation1, new PropertyPath(LargeDelayedToolView.MarginProperty));
            Storyboard.SetTargetProperty(animation2, new PropertyPath(LargeDelayedToolView.OpacityProperty));
            storyBoard.Children.Add(animation1);
            storyBoard.Children.Add(animation2);
            storyBoard.Begin();
        }

        protected override CardView RemoveDelayedTool(Card card)
        {
            return base.RemoveDelayedTool(card);
        }

        protected override void AddEquipment(CardView card)
        {
            Equipment equip = card.Card.Type as Equipment;

            if (equip == null)
            {
                throw new ArgumentException("Cannot add non-equip to equip area.");
            }

            SmallEquipView equipLabel = new SmallEquipView();
            equipLabel.DataContext = card.CardViewModel;
            
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

            Point dest = targetArea.TranslatePoint(new Point(targetArea.Width / 2, targetArea.Height / 2),
                                                   ParentGameView.GlobalCanvas);
            dest.Offset(-card.Width / 2, -card.Height / 2);
            card.Position = dest;
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
            Equipment equip = card.Type as Equipment;

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
            result.Opacity = 0;
            Point dest = targetArea.TranslatePoint(new Point(0, 0), ParentGameView.GlobalCanvas);
            result.Position = dest;
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
        #endregion // PlayerInfoViewBase Members
    }
}
