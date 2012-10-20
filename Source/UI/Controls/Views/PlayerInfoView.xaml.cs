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
    }
}
