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

using Sanguosha.Core.Players;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Games;
using Sanguosha.Core.Skills;
using System.Threading;
using System.Timers;
using System.Collections.ObjectModel;
using Sanguosha.UI.Animations;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Interactivity;
using Microsoft.Expression.Interactivity.Layout;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Battle.Cards;
using Xceed.Wpf.Toolkit;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Sanguosha.Core.Heroes;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for GameTable.xaml
    /// </summary>
    public partial class GameView : UserControl, INotificationProxy
    {
        #region Private Members
        protected static int[][] regularSeatIndex;
        protected static int[][] pk3v3SeatIndex;
        protected static int[][] pk1v3SeatIndex;
        private IList<StackPanel> stackPanels;
        private IList<RadioButton> radioLogs;
        private IList<FlowDocument> logDocs;
        private ObservableCollection<PlayerView> profileBoxes;
        private IDictionary<Player, PlayerViewBase> playersMap;
        private GameLogs gameLogs;
        #endregion

        #region Constructors

        static GameView()
        {
            // Layout:
            //    col1           col2
            // _______________________
            // |_Y_|______4_______|_X_| row1
            // |   |              |   |
            // | 2 |    table     | 0 | 
            // |___|______________|___|
            // |      dashboard       |
            // ------------------------
            // 1 = Y4X, 3=0X, 5=2Y

            regularSeatIndex = new int[][]
            {
                new int[]{4, 0, 0, 0, 0, 0, 0, 0, 0},    // 2 players
                new int[]{3, 5, 0, 0, 0, 0, 0, 0, 0}, // 3 players
                new int[]{3, 4, 5, 0, 0, 0, 0, 0, 0},
                new int[]{0, 4, 4, 2, 0, 0, 0, 0, 0},
                new int[]{0, 4, 4, 4, 2, 0, 0, 0, 0},
                new int[]{3, 3, 4, 4, 5, 5, 0, 0, 0},
                new int[]{3, 3, 4, 4, 4, 5, 5, 0, 0}, // 8 players
                new int[]{0, 0, 1, 1, 1, 1, 2, 2, 0}, // 9 players
                new int[]{0, 0, 1, 1, 1, 1, 1, 2, 2} // 10 players
            };
            pk3v3SeatIndex = new int[][]
            {
                new int[]{1, 1, 1}, // if self is lubu
                new int[]{0, 0, 4},
                new int[]{0, 4, 2},
                new int[]{4, 2, 2}
            };
            pk1v3SeatIndex = new int[][]
            {
                new int[]{0, 4, 4, 4, 2}, // lord        
                new int[]{4, 4, 4, 2, 2}, // rebel (left), same with loyalist (left)
                new int[]{0, 0, 4, 4, 4} // loyalist (right), same with rebel (right)
            };
        }

        public GameView()
        {
            InitializeComponent();
            // Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 120 });
            stackPanels = new List<StackPanel>() { stackPanel0, stackPanel1, stackPanel2, stackPanel3, stackPanel4, stackPanel5 };
            radioLogs = new List<RadioButton>() { rbLog0, rbLog1, rbLog2, rbLog3, rbLog4, rbLog5, rbLog6, rbLog7, rbLog8, rbLog9, rbLog10 };
            profileBoxes = new ObservableCollection<PlayerView>();
            playersMap = new Dictionary<Player, PlayerViewBase>();
            mainPlayerPanel.ParentGameView = this;
            discardDeck.ParentCanvas = this.GlobalCanvas;
            this.DataContextChanged += GameView_DataContextChanged;
            this.SizeChanged += GameView_SizeChanged;
            gameLogs = new GameLogs();
            logDocs = new List<FlowDocument>() { gameLogs.GlobalLog };
            rtbLog.Document = gameLogs.GlobalLog;
            for (int i = 0; i < 10; i++)
            {
                logDocs.Add(new FlowDocument());
            }
            for (int i = 0; i < 11; i++)
            {
                radioLogs[i].Checked += (o, e) =>
                {
                    if (!radioLogs.Contains(o)) return;
                    rtbLog.Document = logDocs[radioLogs.IndexOf(o as RadioButton)];
                    rtbLog.ScrollToEnd();
                };
            }
            pinDianBox.ResultShown += (o, e) =>
            {
                pinDianWindow.Close();
            };
            mainPlayerPanel.SetAnimationCenter(mainPlayerAnimationCenter);
            chatEventHandler = new ChatEventHandler(LobbyModel_OnChat);
            LobbyViewModel.Instance.OnChat += chatEventHandler;
        }

        Dictionary<KeyValuePair<Player, Player>, Line> _cueLines;
        Dictionary<KeyValuePair<Player, Player>, Timeline> _lineUpAnimations;
        // Dictionary<KeyValuePair<Player, Player>, Line> _cueLineGlows;

        private static int _cueLineZIndex = 100000;

        private void _CreateCueLines()
        {
            if (_cueLines != null)
            {
                foreach (var line in _cueLines.Values)
                {
                    GlobalCanvas.Children.Remove(line);
                }
            }

            _cueLines = new Dictionary<KeyValuePair<Player, Player>, Line>();
            _lineUpAnimations = new Dictionary<KeyValuePair<Player, Player>, Timeline>();
            foreach (var source in playersMap.Keys)
            {
                foreach (var target in playersMap.Keys)
                {
                    if (source == target) continue;
                    var key = new KeyValuePair<Player, Player>(source, target);
                    Line line = new Line();
                    line.StrokeDashCap = PenLineCap.Triangle;
                    line.StrokeThickness = 1;
                    line.Stroke = Resources["indicatorLineBrush"] as Brush;
                    line.Effect = new DropShadowEffect() { ShadowDepth = 0, BlurRadius = 3, Color = Colors.White };
                    line.Visibility = Visibility.Collapsed;
                    line.SetValue(Canvas.ZIndexProperty, _cueLineZIndex);
                    /* line2.Stroke = Resources["indicatorLineGlowBrush"] as Brush; */
                    _cueLines.Add(key, line);

                    var anim1 = new DoubleAnimation();
                    anim1.Duration = _lineUpDuration;
                    Storyboard.SetTarget(anim1, line);
                    Storyboard.SetTargetProperty(anim1, new PropertyPath(Line.StrokeDashOffsetProperty));
                    var anim2 = new ObjectAnimationUsingKeyFrames();
                    anim2.Duration = _lineUpDuration;
                    anim2.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Visible, KeyTime.FromPercent(0)));
                    anim2.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Collapsed, KeyTime.FromPercent(1)));
                    Storyboard.SetTarget(anim2, line);
                    Storyboard.SetTargetProperty(anim2, new PropertyPath(Line.VisibilityProperty));

                    Storyboard animation = new Storyboard();
                    animation.FillBehavior = FillBehavior.Stop;
                    animation.Children.Add(anim1);
                    animation.Children.Add(anim2);
                    animation.Duration = _lineUpDuration;

                    _lineUpAnimations.Add(key, animation);

                    GlobalCanvas.Children.Add(line);
                }
            }
        }

        private void _ResizeCueLines()
        {
            gridRoot.UpdateLayout();
            foreach (var source in playersMap.Keys)
            {
                foreach (var target in playersMap.Keys)
                {
                    if (source == target) continue;
                    var key = new KeyValuePair<Player, Player>(source, target);
                    Line line = _cueLines[key];
                    var src = playersMap[source];
                    var dst = playersMap[target];
                    var srcPoint = src.TranslatePoint(new Point(src.ActualWidth / 2, src.ActualHeight / 2), GlobalCanvas);
                    var dstPoint = dst.TranslatePoint(new Point(dst.ActualWidth / 2, dst.ActualHeight / 2), GlobalCanvas);
                    line.X1 = srcPoint.X;
                    line.X2 = dstPoint.X;
                    line.Y1 = srcPoint.Y;
                    line.Y2 = dstPoint.Y;
                    double distance = Math.Sqrt((srcPoint.X - dstPoint.X) * (srcPoint.X - dstPoint.X) + (srcPoint.Y - dstPoint.Y) * (srcPoint.Y - dstPoint.Y));
                    line.StrokeDashArray = new DoubleCollection() { distance * 2.0, 10000d };
                    line.StrokeDashOffset = distance * 2;

                    var animation = (_lineUpAnimations[key] as Storyboard).Children[0] as DoubleAnimation;
                    animation.From = distance * 2.0;
                    animation.To = -distance;
                }
            }
        }

        private ChatEventHandler chatEventHandler;

        void LobbyModel_OnChat(string userName, string msg)
        {
            var player = GameModel.PlayerModels.FirstOrDefault(p => p.Account != null && p.Account.UserName == userName);
            string heroName = string.Empty;
            if (player != null && player.Hero != null) heroName = LogFormatter.Translate(player.Hero);
            chatBox.Document.Blocks.Add(LogFormatter.RichTranslateChat(heroName, userName, msg));
            chatBox.ScrollToEnd();
        }

        void GameView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _Resize(e.NewSize);
        }
        #endregion

        #region Fields

        public Canvas GlobalCanvas
        {
            get
            {
                return gameGlobalCanvas;
            }
        }

        public GameViewModel GameModel
        {
            get
            {
                return DataContext as GameViewModel;
            }
        }

        #endregion

        #region Events
        public event EventHandler OnGameCompleted;
        public event EventHandler OnUiAttached;
        #endregion

        #region Private Functions

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GameSoundPlayer.PlayBackgroundMusic(GameSoundLocator.GetBgm());
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LobbyViewModel.Instance.OnChat -= chatEventHandler;
            CardView.ClearCache();
            if (GameModel != null)
            {
                var model = GameModel;
                foreach (var playerModel in GameModel.PlayerModels)
                {
                    playerModel.Player = null;
                }
                model.PropertyChanged -= model_PropertyChanged;
                model.Game.PropertyChanged -= _game_PropertyChanged;
                foreach (var playerModel in model.PlayerModels)
                {
                    playerModel.PropertyChanged -= _player_PropertyChanged;
                }
                model.MainPlayerModel.PropertyChanged -= mainPlayer_PropertyChanged;
                GameModel.Game = null;
            }
            this.DataContext = null;

        }

        private void _Resize(Size size)
        {
            if (!(size.Width > 0 && size.Height > 0)) return;
            if (profileBoxes.Count == 0)
            {
                return;
            }

            Size tableBounds = new Size();
            tableBounds.Width = size.Width - gridRoot.ColumnDefinitions[1].Width.Value;
            tableBounds.Height = size.Height - gridRoot.RowDefinitions[1].Height.Value -
                                 gridTable.Margin.Bottom;

            // Adjust width/height of all controls
            int rows = Math.Max(
                Math.Max(stackPanel0.Children.Count + 1, stackPanel2.Children.Count + 1),
                Math.Max(stackPanel3.Children.Count, stackPanel5.Children.Count));

            int columns = Math.Max(
                stackPanel1.Children.Count, stackPanel4.Children.Count + 2);

            double width = Math.Min((tableBounds.Width - _minHSpacing * (columns - 1)) / columns,
                                     profileBoxes[0].MaxWidth);
            double height = Math.Min((tableBounds.Height - _minVSpacing * (rows - 1)) / rows,
                                     profileBoxes[0].MaxHeight);
            double ratio = (double)Resources["PlayerInfoView.WidthHeightRatio"];

            width = Math.Min(width, height * ratio + 20);
            height = (width - 20) / ratio;

            double hSpacing = (tableBounds.Width - width * columns) / columns;
            double vSpacing = (tableBounds.Height - height * rows) / (rows + 4);

            foreach (var box in profileBoxes)
            {
                box.Width = width;
                box.Height = height;
            }

            _AdjustSpacing(hSpacing, vSpacing);

            gridTable.RowDefinitions[0].Height = new GridLength(height);
            gridTable.ColumnDefinitions[0].Width = new GridLength(width);
            gridTable.ColumnDefinitions[2].Width = new GridLength(width);

            // Expand infoPanel if table is wide enough
            int thresh = (int)Resources["GameView.InfoPanelExpansionThresholdWidth"];
            if (size.Width > thresh)
            {
                mainPlayerPanel.SetValue(Grid.ColumnSpanProperty, 1);
                infoPanel.SetValue(Grid.RowSpanProperty, 2);
            }
            else
            {
                mainPlayerPanel.SetValue(Grid.ColumnSpanProperty, 2);
                infoPanel.SetValue(Grid.RowSpanProperty, 1);
            }

            foreach (var playerView in playersMap.Values)
            {
                playerView.UpdateCardAreas();
            }
            discardDeck.RearrangeCards();
            _ResizeCueLines();
            InvalidateMeasure();
            InvalidateArrange();
        }

        private void _CreatePlayerInfoView(int indexInGameModel)
        {
            GameViewModel model = GameModel;
            var playerModel = model.PlayerModels[indexInGameModel];
            var playerView = new PlayerView() { ParentGameView = this, DataContext = playerModel };
            playerView.OnRequestSpectate += playerView_OnRequestSpectate;
            profileBoxes.Insert(indexInGameModel - 1, playerView);
            if (!playersMap.ContainsKey(playerModel.Player))
            {
                playersMap.Add(playerModel.Player, playerView);
            }
            else
            {
                playersMap[playerModel.Player] = playerView;
            }
        }

        void playerView_OnRequestSpectate(object sender, EventArgs e)
        {
            var view = sender as PlayerView;
            Trace.Assert(view != null);

            GameModel.MainPlayerSeatNumber = GameModel.Game.Players.IndexOf(view.PlayerModel.Player);
        }

        private void mainPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PlayerViewModel model = sender as PlayerViewModel;
            Trace.Assert(model != null, "Property change is expected to be associate with a PlayerViewModel");
            if (e.PropertyName == "TimeOutSeconds")
            {
                Duration duration = new Duration(TimeSpan.FromSeconds(model.TimeOutSeconds));
                DoubleAnimation doubleanimation = new DoubleAnimation(100d, 0d, duration);
                progressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
            }
            else if (e.PropertyName == "CurrentPrivateDeck")
            {
                _constructPlayerCurrentPrivateDeck(model);
            }
        }

        ChildWindow _privateDeckChoiceWindow;

        private void _constructPlayerCurrentPrivateDeck(PlayerViewModel model)
        {
            if (model.CurrentPrivateDeck != null)
            {
                Trace.Assert(model.CurrentPrivateDeck.Cards != null);
                if (_privateDeckChoiceWindow != null)
                {
                    gridRoot.Children.Remove(_privateDeckChoiceWindow);
                }

                _privateDeckChoiceWindow = new ChildWindow();
                _privateDeckChoiceWindow.Template = Resources["BlackWindowStyle"] as ControlTemplate;
                _privateDeckChoiceWindow.MaxWidth = 800;
                _privateDeckChoiceWindow.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                _privateDeckChoiceWindow.CloseButtonVisibility = Visibility.Collapsed;
                _privateDeckChoiceWindow.Effect = new DropShadowEffect() { BlurRadius = 10d };
                _privateDeckChoiceWindow.WindowStartupLocation = Xceed.Wpf.Toolkit.WindowStartupLocation.Center;
                string title = PromptFormatter.Format(new CardChoicePrompt("PrivateDeck", model.Player, model.CurrentPrivateDeck.TraslatedName));
                _privateDeckChoiceWindow.Caption = title;

                var box = new PrivateDeckBox();
                box.DataContext = model.CurrentPrivateDeck.Cards;
                _privateDeckChoiceWindow.Content = box;

                gridRoot.Children.Add(_privateDeckChoiceWindow);
                _privateDeckChoiceWindow.Show();
            }
            else
            {
                gridRoot.Children.Remove(_privateDeckChoiceWindow);
                _privateDeckChoiceWindow = null;
            }
        }

        private void GameView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                GameViewModel oldModel = e.OldValue as GameViewModel;
                if (oldModel == null || oldModel.Game == null) return;
                oldModel.PropertyChanged -= model_PropertyChanged;
                oldModel.Game.PropertyChanged -= _game_PropertyChanged;
                foreach (var playerModel in oldModel.PlayerModels)
                {
                    playerModel.PropertyChanged -= _player_PropertyChanged;
                }
                oldModel.MainPlayerModel.PropertyChanged -= mainPlayer_PropertyChanged;
            }

            profileBoxes.Clear();
            playersMap.Clear();
            GameViewModel model = GameModel;

            if (model == null) return;

            for (int i = 1; i < model.PlayerModels.Count; i++)
            {
                _CreatePlayerInfoView(i);
            }
            playersMap.Add(model.MainPlayerModel.Player, mainPlayerPanel);
            RearrangeSeats();
            _CreateCueLines();

            model.PropertyChanged += model_PropertyChanged;
            model.Game.PropertyChanged += _game_PropertyChanged;
            Trace.Assert(model.MainPlayerModel != null, "Main player must exist.");

            // Initialize game logs.
            gameLogs.Logs.Clear();
            int count = model.PlayerModels.Count;
            for (int i = 0; i < 11; i++)
            {
                var rb = radioLogs[i];
                if (i <= count)
                {
                    rb.Visibility = Visibility.Visible;
                    rb.IsEnabled = true;
                }
                else
                {
                    rb.Visibility = Visibility.Hidden;
                    rb.IsEnabled = false;
                }
            }
            radioLogs[0].IsChecked = true;

            for (int i = 0; i < count; i++)
            {
                model.PlayerModels[i].PropertyChanged += _player_PropertyChanged;
            }
            _Resize(new Size(this.ActualWidth, this.ActualHeight));
        }

        ChildWindow cardChoiceWindow;
        void _player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PlayerViewModel model = sender as PlayerViewModel;
            if (model == null || model.Player == null) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                int count = GameModel.PlayerModels.Count;
                if (e.PropertyName == "IsCardChoiceQuestionShown")
                {
                    if (model.IsCardChoiceQuestionShown)
                    {
                        if (cardChoiceWindow != null)
                        {
                            gridRoot.Children.Remove(cardChoiceWindow);
                        }
                        cardChoiceWindow = new ChildWindow();
                        cardChoiceWindow.Template = Resources["BlackWindowStyle"] as ControlTemplate;
                        cardChoiceWindow.MaxWidth = 800;
                        cardChoiceWindow.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                        cardChoiceWindow.CloseButtonVisibility = model.CardChoiceModel.CanClose ? Visibility.Visible : Visibility.Collapsed;
                        cardChoiceWindow.Effect = new DropShadowEffect() { BlurRadius = 10d };
                        cardChoiceWindow.Caption = model.CardChoiceModel.Prompt;
                        var box = CardChoiceBoxSelector.CreateBox(model.CardChoiceModel);
                        if (box is CardArrangeBox)
                        {
                            (box as CardArrangeBox).OnCardMoved += (s1, s2, d1, d2) =>
                            {
                                var callback = model.CurrentCardChoiceRearrangeCallback;
                                if (callback != null)
                                {
                                    callback(new CardRearrangement(s1, s2, d1, d2));
                                }
                            };
                        }
                        gridRoot.Children.Add(cardChoiceWindow);
                        cardChoiceWindow.WindowStartupLocation = Xceed.Wpf.Toolkit.WindowStartupLocation.Center;
                        cardChoiceWindow.Content = box;
                        cardChoiceWindow.Show();
                    }
                    else if (cardChoiceWindow != null)
                    {
                        cardChoiceWindow.Content = null;
                        cardChoiceWindow.Close();
                        gridRoot.Children.Remove(cardChoiceWindow);
                        cardChoiceWindow = null;
                    }
                }
                else if (e.PropertyName == "Role")
                {
                    int index;
                    for (index = 0; index < count; index++)
                    {
                        var playerModel = GameModel.PlayerModels[index];
                        if (playerModel == model) break;
                    }
                    Trace.Assert(index < count);

                    if (model.Player.Role == Role.Ruler)
                    {
                        gameLogs.Logs.Clear();
                        for (int i = 0; i < count; i++)
                        {
                            gameLogs.Logs.Add(GameModel.PlayerModels[(i + index) % count].Player, logDocs[i + 1]);
                        }
                    }
                }
                else if (e.PropertyName == "Hero")
                {
                    gameLogs.AppendPickHeroLog(model.Player, true);
                }
                else if (e.PropertyName == "Hero2")
                {
                    gameLogs.AppendPickHeroLog(model.Player, false);
                }
            });
        }

        void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MainPlayerSeatNumber")
            {
                RearrangeSeats();
                _ResizeCueLines();
            }
        }

        void _game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                string name = e.PropertyName;
                if (name == "CurrentPlayer")
                {
                    gameLogs.AppendSeparator();
                    discardDeck.UnlockCards();
                }
            });
        }

        private void _RearrangeSeats()
        {
            GameViewModel model = GameModel;
            Trace.Assert(model.PlayerModels.Count == profileBoxes.Count + 1);
            int playerCount = model.PlayerModels.Count;

            // First remove main player
            for (int j = 0; j < profileBoxes.Count; j++)
            {
                var playerView = profileBoxes[j];
                if (playerView.PlayerModel == model.MainPlayerModel)
                {
                    profileBoxes.RemoveAt(j);
                    break;
                }
            }

            for (int i = 1; i < playerCount; i++)
            {
                var playerModel = model.PlayerModels[i];
                bool found = false;
                for (int j = 0; j < profileBoxes.Count; j++)
                {
                    var playerView = profileBoxes[j];
                    if (playerModel == playerView.PlayerModel)
                    {
                        if (i - 1 < j)
                        {
                            profileBoxes.Move(j, i - 1);
                        }
                        playersMap[playerModel.Player] = playerView;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    _CreatePlayerInfoView(i);
                }
            }

            var oldMainPlayer = mainPlayerPanel.DataContext as PlayerViewModel;
            if (oldMainPlayer != null)
            {
                oldMainPlayer.PropertyChanged -= mainPlayer_PropertyChanged;
            }
            mainPlayerPanel.DataContext = model.MainPlayerModel;
            model.MainPlayerModel.PropertyChanged += mainPlayer_PropertyChanged;
            playersMap[model.MainPlayerModel.Player] = mainPlayerPanel;
        }

        #endregion

        #region Public Functions
        public void PlayAnimation(AnimationBase animation)
        {
            animation.HorizontalAlignment = HorizontalAlignment.Center;
            animation.VerticalAlignment = VerticalAlignment.Center;
            animationCenter.Children.Add(animation);
            animation.Start();
        }
        #endregion

        #region Layout Related
        public void RearrangeSeats()
        {
            GameViewModel model = GameModel;

            int[] seatMap = null;

            int numPlayers = model.Game.Players.Count;

            if (numPlayers == 0) return;

            if (numPlayers >= 10)
            {
                throw new NotImplementedException("Only fewer than 10 players are supported by UI.");
            }

            _RearrangeSeats();

            // Generate seat map.
            switch (model.TableLayout)
            {
                case GameTableLayout.Regular:
                    seatMap = regularSeatIndex[numPlayers - 2];
                    break;
                case GameTableLayout.Pk3v3:
                    seatMap = pk3v3SeatIndex[(model.MainPlayerSeatNumber - 1) % 3];
                    break;
                case GameTableLayout.Pk1v3:
                    seatMap = pk1v3SeatIndex[model.MainPlayerSeatNumber - 1];
                    break;
            }

            // Add profile boxes to stack panels.
            foreach (StackPanel panel in stackPanels)
            {
                panel.Children.Clear();
            }

            for (int i = 0; i < profileBoxes.Count; i++)
            {
                int seat = seatMap[i];
                if (seat % 3 == 2)
                {
                    stackPanels[seat].Children.Add(profileBoxes[i]);
                }
                else
                {
                    stackPanels[seat].Children.Insert(0, profileBoxes[i]);
                }
                if (seat % 3 == 0)
                {
                    profileBoxes[i].FlowDirection = FlowDirection.RightToLeft;
                }
                else
                {
                    profileBoxes[i].FlowDirection = FlowDirection.LeftToRight;
                }
            }
        }

        private void _AdjustSpacing(double hSpacing, double vSpacing)
        {
            foreach (var panel in stackPanels)
            {
                if (panel.Orientation == Orientation.Vertical)
                {
                    foreach (var box in panel.Children)
                    {
                        PlayerView view = box as PlayerView;
                        view.Margin = new Thickness(0, vSpacing / 2, 0, vSpacing / 2);
                    }
                }
                else
                {
                    foreach (var box in panel.Children)
                    {
                        PlayerView view = box as PlayerView;
                        view.Margin = new Thickness(hSpacing / 2, 0, hSpacing / 2, 0);
                    }
                }
            }
        }

        private static int _minHSpacing = 0;
        private static int _minVSpacing = 2;
        #endregion

        #region Card Movement

        private IDeckContainer _GetMovementDeck(DeckPlace place)
        {
            if (place.Player != null && place.DeckType != DeckType.JudgeResult)
            {
                PlayerViewBase playerView = playersMap[place.Player];
                return playerView;
            }
            else
            {
                return discardDeck;
            }
        }

        #endregion

        #region Game Event Notification
        private static Duration _lineUpDuration = new Duration(TimeSpan.FromSeconds(0.8d));

        private void _LineUp(Player source, IList<Player> targets)
        {

            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                Storyboard lineUpGroup = new Storyboard();
                foreach (var target in targets)
                {
                    if (source == target) continue;
                    var key = new KeyValuePair<Player, Player>(source, target);
                    var animation = _lineUpAnimations[key];
                    lineUpGroup.Children.Add(animation);
                }
                lineUpGroup.AccelerationRatio = 0.6;
                lineUpGroup.Begin();
            });
        }


        #endregion

        #region Private Functions
        private void _AppendKeyEventLog(Paragraph log)
        {
            var doc = new FlowDocument();
            var para = log;
            if (para.Inlines.Count == 0) return;
            doc.Blocks.Add(para);
            keyEventLog.AddLog(doc);
        }

        private void _AppendKeyEventLog(Prompt custom, bool useUICard = true)
        {
            Paragraph para = new Paragraph();
            para.Inlines.AddRange(LogFormatter.TranslateLogEvent(custom, useUICard));
            _AppendKeyEventLog(para);
        }

        private void _AppendKeyEventLog(ActionLog log)
        {
            _AppendKeyEventLog(LogFormatter.RichTranslateKeyLog(log));
        }
        #endregion

        #region INotificationProxy

        public void NotifyCardMovement(List<CardsMovement> moves)
        {
            if (ViewModelBase.IsDetached) return;

            bool doWeaponSound = false;
            bool doArmorSound = false;
            bool doHorseSound = false;
            foreach (CardsMovement move in moves)
            {
                if (move.Helper.IsWuGu)
                {
                    Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                    {
                        // WuGuModel can be null if we missed NotifyWuGuStart during reconnection.
                        if (GameModel.WuGuModel == null) return;
                        Trace.Assert(GameModel.WuGuModel != null);
                        Trace.Assert(move.Cards.Count == 1);
                        Trace.Assert(move.To.Player != null);
                        Trace.Assert(move.Cards[0].Id != -1);

                        var cardModel = GameModel.WuGuModel.Cards.FirstOrDefault(c => c.Card.Id == move.Cards[0].Id);
                        Trace.Assert(cardModel != null);
                        cardModel.IsEnabled = false;
                        cardModel.Footnote = LogFormatter.Translate(move.To.Player);
                        cardModel.IsFootnoteVisible = true;
                    });
                }

                var cardsToAdd = new List<CardView>();
                var cardsRemoved = new Dictionary<DeckPlace, List<Card>>();

                foreach (Card card in move.Cards)
                {
                    var place = card.PlaceOverride ?? card.Place;
                    card.PlaceOverride = null;
                    if (!cardsRemoved.ContainsKey(place))
                    {
                        cardsRemoved.Add(place, new List<Card>());
                    }
                    cardsRemoved[place].Add(card);
                    if (move.To.DeckType == DeckType.Equipment)
                    {
                        if (card.Type is Weapon) doWeaponSound = true;
                        else if (card.Type is Armor) doArmorSound = true;
                        else if (card.Type is DefensiveHorse || card.Type is OffensiveHorse) doHorseSound = true;
                    }
                }

                foreach (var stackCards in cardsRemoved)
                {
                    IDeckContainer deck = _GetMovementDeck(stackCards.Key);
                    IList<CardView> cards = null;
                    Trace.Assert(move.Helper != null);
                    if (!move.Helper.IsFakedMove || move.Helper.AlwaysShowLog)
                    {
                        Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate()
                        {
                            gameLogs.AppendCardMoveLog(stackCards.Value, stackCards.Key, move.To);
                        });
                    }
                    Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                    {
                        cards = deck.RemoveCards(stackCards.Key.DeckType, stackCards.Value);
                    });
                    Trace.Assert(cards != null);
                    foreach (var card in cards)
                    {
                        Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                        {
                            card.Update();
                        });
                    }
                    cardsToAdd.AddRange(cards);
                }

                Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                {
                    _GetMovementDeck(move.To).AddCards(move.To.DeckType, cardsToAdd, move.Helper.IsFakedMove);
                });
            }
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                rtbLog.ScrollToEnd();
            });
            if (doWeaponSound)
            {
                Uri uri = GameSoundLocator.GetSystemSound("Weapon");
                GameSoundPlayer.PlaySoundEffect(uri);
            }
            if (doArmorSound)
            {
                Uri uri = GameSoundLocator.GetSystemSound("Armor");
                GameSoundPlayer.PlaySoundEffect(uri);
            }
            if (doHorseSound)
            {
                Uri uri = GameSoundLocator.GetSystemSound("Horse");
                GameSoundPlayer.PlaySoundEffect(uri);
            }
        }

        public void NotifyDamage(Player source, Player target, int magnitude, DamageElement element)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                foreach (var profile in playersMap.Values)
                {
                    if (profile.PlayerModel.Player == target)
                    {
                        profile.PlayAnimation(new DamageAnimation(), 1, new Point(0, 0));
                        profile.Tremble();
                    }
                }
                gameLogs.AppendDamageLog(source, target, magnitude, element);
                rtbLog.ScrollToEnd();
                Uri uri = GameSoundLocator.GetSystemSound("Damage");
                GameSoundPlayer.PlaySoundEffect(uri);
            });
        }

        private static ResourceDictionary equipAnimationResources = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Animations;component/EquipmentAnimations.xaml") };
        private static ResourceDictionary baseCardAnimationResources = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Animations;component/BaseCardAnimations.xaml") };

        public void NotifySkillUse(ActionLog log)
        {
            if (ViewModelBase.IsDetached) return;
            Trace.Assert(log.Source != null);
            PlayerViewBase player = playersMap[log.Source];
            bool soundPlayed = false;
            if (log.SkillAction != null)
            {
                string key1 = string.Format("{0}.Animation", log.SkillAction.GetType().Name);
                string key2 = key1 + ".Offset";
                bool animPlayed = false;
                lock (equipAnimationResources)
                {
                    if (equipAnimationResources.Contains(key1))
                    {
                        AnimationBase animation = null;
                        Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                        {
                            animation = equipAnimationResources[key1] as AnimationBase;
                        });
                        if (animation != null && animation.Parent == null)
                        {
                            Point offset = new Point(0, 0);
                            if (equipAnimationResources.Contains(key2))
                            {
                                offset = (Point)equipAnimationResources[key2];
                            }
                            player.PlayAnimationAsync(animation, 0, offset);
                            animPlayed = true;
                        }
                    }
                }
                if (log.SkillAction.IsSingleUse || log.SkillAction.IsAwakening)
                {
                    if (log.SkillAction.IsAwakening) log.Source[Player.Awakened]++;
                    Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate()
                    {
                        ExcitingSkillAnimation anim = new ExcitingSkillAnimation();
                        anim.SkillName = log.SkillAction.GetType().Name;
                        anim.HeroName = log.SkillAction.HeroTag.Name;
                        gridRoot.Children.Add(anim);
                        anim.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        anim.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        anim.Start();
                    });
                    animPlayed = true;
                }
                if (!animPlayed && player != mainPlayerPanel)
                {
                    string s = LogFormatter.Translate(log.SkillAction);
                    if (s != string.Empty)
                    {
                        ZoomTextAnimation anim = null;
                        Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                        {
                            anim = new ZoomTextAnimation() { Text = s };
                        });
                        Trace.Assert(anim != null);
                        player.PlayAnimationAsync(anim, 1, new Point(0, 0));
                        animPlayed = true;
                    }
                }
                string soundKey = log.SkillAction.GetType().Name;
                Uri uri = GameSoundLocator.GetSkillSound(soundKey, log.SpecialEffectHint);
                GameSoundPlayer.PlaySoundEffect(uri);
                soundPlayed = uri != null;
            }
            else if (log.GameAction == GameAction.None)
            {
                bool? isMale = null;
                if (log.Source != null) isMale = !log.Source.IsFemale;
                Uri cardSoundUri = GameSoundLocator.GetCardSound(log.CardAction.Type.CardType, isMale);
                GameSoundPlayer.PlaySoundEffect(cardSoundUri);
                soundPlayed = cardSoundUri != null;
            }
            if (log.CardAction != null && log.GameAction != GameAction.None)
            {
                if (log.CardAction.Type is TieSuoLianHuan)
                {
                    foreach (var p in log.Targets)
                    {
                        Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate()
                        {
                            playersMap[p].PlayIronShackleAnimation();
                        });
                    }
                }
                else
                {
                    ICard c = log.CardAction;
                    string key1;
                    key1 = string.Format("{0}.{1}.Animation", c.Type.GetType().Name, c.SuitColor == SuitColorType.Red ? "Red" : "Black");
                    if (!baseCardAnimationResources.Contains(key1))
                    {
                        key1 = string.Format("{0}.Animation", c.Type.GetType().Name);
                    }
                    string key2 = key1 + ".Offset";
                    lock (baseCardAnimationResources)
                    {
                        if (baseCardAnimationResources.Contains(key1))
                        {
                            AnimationBase animation = null;
                            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                            {
                                animation = baseCardAnimationResources[key1] as AnimationBase;
                            });
                            if (animation != null && animation.Parent == null)
                            {
                                Point offset = new Point(0, 0);
                                if (baseCardAnimationResources.Contains(key2))
                                {
                                    offset = (Point)baseCardAnimationResources[key2];
                                }
                                player.PlayAnimationAsync(animation, 0, offset);
                            }
                        }
                    }
                }

                bool? isMale = null;
                if (log.Source != null) isMale = !log.Source.IsFemale;
                Uri cardSoundUri = GameSoundLocator.GetCardSound(log.CardAction.Type.CardType, isMale);
                var card = log.CardAction as Card;
                if (card != null)
                {
                    bool play = true;
                    if (card.Log != null && card.Log.SkillAction is IEquipmentSkill)
                    {
                        Uri uri = GameSoundLocator.GetSkillSound(card.Log.SkillAction.GetType().Name);
                        if (uri != null) play = false;
                    }
                    if (play && !soundPlayed) GameSoundPlayer.PlaySoundEffect(cardSoundUri);
                }
            }

            if (log.GameAction != GameAction.None || log.SkillAction != null && log.CardAction == null || log.ShowCueLine)
            {
                if (log.Targets.Count > 0)
                {
                    _LineUp(log.Source, log.Targets);
                    foreach (var target in log.Targets)
                    {
                        target.IsTargeted = true;
                    }
                }

                if (log.Targets.Count == 1 && log.SecondaryTargets != null && log.SecondaryTargets.Count > 0)
                {
                    _LineUp(log.Targets[0], log.SecondaryTargets);
                    foreach (var target in log.SecondaryTargets)
                    {
                        target.IsTargeted = true;
                    }
                }
            }

            if (!log.SkillSoundOnly)
            {
                Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                {
                    gameLogs.AppendLog(log);
                    rtbLog.ScrollToEnd();
                });

                Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                {
                    _AppendKeyEventLog(log);
                });
            }
        }

        public void NotifyUiAttached()
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                ViewModelBase.AttachAll();
                foreach (var player in playersMap.Values)
                {
                    player.UpdateCards();
                }
                busyIndicator.IsBusy = false;
                var handler = OnUiAttached;
                if (handler != null)
                {
                    handler(this, new EventArgs());
                }
            });
        }

        public void NotifyUiDetached()
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                busyIndicator.BusyContent = Resources["Busy.Reconnecting"];
                busyIndicator.IsBusy = true;
            });
            ViewModelBase.DetachAll();
        }

        private ChildWindow _showHandCardsWindow;

        public void NotifyShowCardsStart(Player p, List<Card> cards)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                if (_showHandCardsWindow != null)
                {
                    gridRoot.Children.Remove(_showHandCardsWindow);
                }

                _showHandCardsWindow = new ChildWindow();
                _showHandCardsWindow.Template = Resources["BlackWindowStyle"] as ControlTemplate;
                _showHandCardsWindow.MaxWidth = 800;
                _showHandCardsWindow.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                _showHandCardsWindow.CloseButtonVisibility = Visibility.Visible;
                _showHandCardsWindow.Effect = new DropShadowEffect() { BlurRadius = 10d };
                _showHandCardsWindow.WindowStartupLocation = Xceed.Wpf.Toolkit.WindowStartupLocation.Center;
                string title = PromptFormatter.Format(new CardChoicePrompt("ShowCards", p));
                _showHandCardsWindow.Caption = title;

                var viewModels = from c in cards select new CardViewModel() { Card = c };

                var box = new PrivateDeckBox();
                box.IsHitTestVisible = false;
                box.Margin = new Thickness(0, -20, 0, 0);
                box.DataContext = new ObservableCollection<CardViewModel>(viewModels);
                _showHandCardsWindow.Content = box;

                gridRoot.Children.Add(_showHandCardsWindow);
                _showHandCardsWindow.Show();

                _showHandCardsWindow.Closed += (o, e) =>
                {
                    GameModel.MainPlayerModel.AnswerEmptyMultichoiceQuestion();
                };
            });
        }

        public void NotifyShowCardsEnd()
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                if (_showHandCardsWindow == null) return;
                gridRoot.Children.Remove(_showHandCardsWindow);
                _showHandCardsWindow = null;
            });
        }

        public void NotifyMultipleChoiceResult(Player p, OptionPrompt answer)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                gameLogs.AppendMultipleChoiceLog(p, PromptFormatter.Format(answer));
            });
        }

        public void NotifyDeath(Player p, Player by)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                var uri = GameSoundLocator.GetDeathSound(p.Hero.Name);
                GameSoundPlayer.PlaySoundEffect(uri);
                gameLogs.AppendDeathLog(p, by);
            });
        }

        public void NotifyActionComplete()
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                foreach (var player in GameModel.PlayerModels)
                {
                    player.Player.IsTargeted = false;
                }
                discardDeck.UnlockCards();
            });
        }

        public void NotifyLoseHealth(Player player, int delta)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                gameLogs.AppendLoseHealthLog(player, delta);
                rtbLog.ScrollToEnd();
                Uri uri = GameSoundLocator.GetSystemSound("Damage");
                GameSoundPlayer.PlaySoundEffect(uri);
            });
        }

        public void NotifyRecoverHealth(Player player, int delta)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                gameLogs.AppendRecoverHealthLog(player, delta);
                rtbLog.ScrollToEnd();
                Uri uri = GameSoundLocator.GetSystemSound("RecoverHealth");
                GameSoundPlayer.PlaySoundEffect(uri);
            });
        }

        public void NotifyReforge(Player p, ICard card)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                Uri uri = GameSoundLocator.GetSystemSound("Reforge");
                GameSoundPlayer.PlaySoundEffect(uri);
                gameLogs.AppendReforgeLog(p, card);
                rtbLog.ScrollToEnd();
            });
        }

        public void NotifyLogEvent(Prompt custom, List<Player> players = null, bool isKeyEvent = true, bool useUICard = true)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                gameLogs.AppendLogEvent(players == null ? Game.CurrentGame.Players : players, custom, useUICard);
                rtbLog.ScrollToEnd();
                if (isKeyEvent) _AppendKeyEventLog(custom, useUICard);
            });
        }

        public void NotifyShowCard(Player p, Card card)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                if (p == null)
                {
                    var cardView = CardView.CreateCard(card);
                    GlobalCanvas.Children.Add(cardView);
                    cardView.CardModel.Footnote = LogFormatter.TranslateCardFootnote(card.Log);
                    discardDeck.AppendCards(new List<CardView>() { cardView });
                    return;
                }

                Trace.Assert(card.Place.Player == p);
                var cards = playersMap[p].RemoveCards(card.Place.DeckType, new List<Card>() { card }, true);
                Trace.Assert(cards.Count == 1);
                cards[0].CardModel.Footnote = LogFormatter.TranslateCardFootnote(new ActionLog() { Source = p, GameAction = GameAction.Show });
                discardDeck.AddCards(DeckType.Discard, cards, false, false);
                gameLogs.AppendShowCardsLog(p, new List<Card>() { card });
                rtbLog.ScrollToEnd();
            });
        }

        public void NotifyCardChoiceCallback(CardRearrangement arrange)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                if (cardChoiceWindow == null) return;
                var box = cardChoiceWindow.Content as CardArrangeBox;
                if (box == null) return;
                box.MoveCard(arrange);
            });
        }

        public void NotifyImpersonation(Player p, Hero impersonator, Hero impersonatedHero, ISkill acquiredSkill)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                var view = playersMap[p];
                var model = view.PlayerModel.GetHeroModel(impersonator);
                Trace.Assert(model != null);
                if (impersonatedHero == null)
                {
                    model.ImpersonatedHeroName = string.Empty;
                    model.ImpersonatedSkill = string.Empty;
                }
                else
                {
                    model.ImpersonatedHeroName = impersonatedHero.Name;
                    model.ImpersonatedSkill = acquiredSkill.GetType().Name;
                }
                view.UpdateImpersonateStatus(model == view.PlayerModel.Hero1Model);
            });
        }

        public void NotifyGameStart()
        {
            if (ViewModelBase.IsDetached) return;
            GameSoundPlayer.PlaySoundEffect(GameSoundLocator.GetSystemSound("GameStart"));

            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                PlayAnimation(new GameStartAnimation());
            });
        }

        public void NotifyJudge(Player p, Card card, ActionLog log, bool? isSuccess, bool isFinalResult)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                CardView cardView = discardDeck.Cards.FirstOrDefault(c => c.Card.Id == card.Id);

                if (cardView == null) return;
                gameLogs.AppendJudgeResultLog(p, card, log, isSuccess, isFinalResult);
                rtbLog.ScrollToEnd();

                if (!isFinalResult || isSuccess == null) return;
                _AppendKeyEventLog(LogFormatter.RichTranslateJudgeResultEffectiveness(p, log, isSuccess == true));

                if (isSuccess == true)
                {
                    cardView.PlayAnimation(new TickAnimation(), new Point(0, 0));
                }
                else
                {
                    cardView.PlayAnimation(new CrossAnimation(), new Point(0, 0));
                }
            });
        }

        public void NotifyWuGuStart(Prompt prompt, DeckPlace place)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                GameModel.WuGuModel = new WuGuChoiceViewModel();
                GameModel.WuGuModel.Prompt = PromptFormatter.Format(prompt);
                bool isFirstRow = true;
                int i = 0;
                int total = Game.CurrentGame.Decks[place].Count;

                foreach (var c in Game.CurrentGame.Decks[place])
                {
                    if (isFirstRow && total > 5 && i >= (total + 1) / 2) isFirstRow = false;
                    var card = new CardViewModel() { Card = c, IsSelectionMode = true, IsEnabled = true };
                    if (isFirstRow) GameModel.WuGuModel.Cards1.Add(card);
                    else GameModel.WuGuModel.Cards2.Add(card);
                    card.OnSelectedChanged += new EventHandler(card_OnSelectedChanged);
                    i++;
                }

                wuGuWindow.Show();
            });
        }

        void card_OnSelectedChanged(object sender, EventArgs e)
        {
            if (ViewModelBase.IsDetached) return;
            GameModel.CurrentActivePlayer.AnswerWuGuChoice((sender as CardViewModel).Card);
        }

        public void NotifyWuGuEnd()
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                wuGuWindow.Close();
                GameModel.WuGuModel = null;
            });
        }


        public void NotifyPinDianStart(Player from, Player to, ISkill reason)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                pinDianWindow.Caption = PromptFormatter.Format(new Prompt("Window.PinDian.Prompt", reason));
                pinDianBox.StartPinDian(from, to);
                pinDianWindow.Show();
            });
        }

        public void NotifyMultipleCardUsageResponded(Player player)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                pinDianBox.OnPinDianCardPlayed(player);
            });
        }

        public void NotifyPinDianEnd(Card c1, Card c2)
        {
            if (ViewModelBase.IsDetached) return;
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                pinDianBox.RevealResult(c1, c2);
            });
        }

        private EventHandler _showGameResultWindowHandler;
        private EventHandler _closeGameResultWindowHandler;

        public void NotifyGameOver(bool isDraw, List<Player> winners)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                Uri uri = GameSoundLocator.GetSystemSound("GameOver");
                GameSoundPlayer.PlaySoundEffect(uri);
                GameSoundPlayer.PlayBackgroundMusic(null);
                List<Player> drawers;
                List<Player> losers;
                if (isDraw)
                {
                    Trace.Assert(winners.Count == 0);
                    drawers = Game.CurrentGame.Players;
                    losers = new List<Player>();
                }
                else
                {
                    drawers = new List<Player>();
                    losers = new List<Player>(Game.CurrentGame.Players.Except(winners));
                }
                bool delayWindow = true;
                if (winners.Contains(GameModel.MainPlayerModel.Player))
                {
                    PlayAnimation(new WinAnimation());
                }
                else if (losers.Contains(GameModel.MainPlayerModel.Player))
                {
                    PlayAnimation(new LoseAnimation());
                }
                else delayWindow = false;
                LobbyViewModel.Instance.OnChat -= chatEventHandler;

                ObservableCollection<GameResultViewModel> model = new ObservableCollection<GameResultViewModel>();
                foreach (Player player in winners.Concat(losers).Concat(drawers))
                {
                    var m = new GameResultViewModel();
                    m.Player = player;
                    if (winners.Contains(player))
                    {
                        m.Result = GameResult.Win;
                        // @todo : need to refactor this to sync it with Game.cs
                        if (player.Role == Role.Defector)
                            m.GainedExperience = "+55";
                        else
                            m.GainedExperience = "+5";
                        m.GainedTechPoints = "+0";
                    }
                    else if (losers.Contains(player))
                    {
                        m.Result = GameResult.Lose;                        
                        m.GainedExperience = "-1";
                        m.GainedTechPoints = "+0";
                    }
                    else if (drawers.Contains(player))
                    {
                        m.Result = GameResult.Draw;                        
                        m.GainedExperience = "+0";
                        m.GainedTechPoints = "+0";
                    }
                    model.Add(m);
                }
                gameResultBox.DataContext = model;

                _closeGameResultWindowHandler = (o, e) =>
                {
                    var handler = OnGameCompleted;
                    if (handler != null)
                    {
                        handler(this, new EventArgs());
                    }
                    gameResultWindow.Closed -= _closeGameResultWindowHandler;
                };
                gameResultWindow.Closed += _closeGameResultWindowHandler;

                if (delayWindow)
                {
                    DispatcherTimer timer = new DispatcherTimer();
                    _showGameResultWindowHandler = (o, e) =>
                    {
                        gameResultWindow.Show();
                        timer.Stop();
                        timer.Tick -= _showGameResultWindowHandler;
                    };

                    timer.Tick += _showGameResultWindowHandler;
                    timer.Interval = TimeSpan.FromSeconds(2);
                    timer.Start();
                }
                else
                {
                    gameResultWindow.Show();
                }
            });
        }
		
        private void btnCloseResultBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            gameResultWindow.Close();
        }
        #endregion

        #region Private Decks
        public void DisplayPrivateDeck(Player player, PrivateDeckViewModel model)
        {
            var choiceModel = new CardChoiceViewModel();
            choiceModel.CanClose = true;
            choiceModel.Prompt = PromptFormatter.Format(new CardChoicePrompt("PrivateDeck", player, model.TraslatedName));
            var lineViewModel = new CardChoiceLineViewModel();
            lineViewModel.DeckName = model.Name;
            lineViewModel.Cards = model.Cards;
            choiceModel.CardStacks.Add(lineViewModel);
            choiceModel.DisplayOnly = true;
            deckDisplayWindow.DataContext = choiceModel;
            deckDisplayWindow.Show();
        }
        #endregion

    }
}
