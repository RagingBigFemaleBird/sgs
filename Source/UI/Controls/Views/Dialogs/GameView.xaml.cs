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
            this.DataContextChanged +=  GameView_DataContextChanged;
            this.SizeChanged += GameView_SizeChanged;
            _mainPlayerPropertyChangedHandler = mainPlayer_PropertyChanged;
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
            chatEventHandler = new ChatEventHandler(LobbyModel_OnChat);
            LobbyViewModel.Instance.OnChat += chatEventHandler;
        }

        private ChatEventHandler chatEventHandler;

        void LobbyModel_OnChat(string userName, string msg)
        {
            var player = GameModel.PlayerModels.FirstOrDefault(p => p.Account != null && p.Account.UserName == userName);
            string heroName = string.Empty;
            if (player.Hero != null) heroName = player.Hero.Name;
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
        #endregion

        #region Private Functions

        private void _Resize(Size size)
        {
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
        }

        private void _CreatePlayerInfoView(int indexInGameModel)
        {
            GameViewModel model = GameModel;
            var playerModel = model.PlayerModels[indexInGameModel];
            var playerView = new PlayerView() { DataContext = playerModel, ParentGameView = this };
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

        private PropertyChangedEventHandler _mainPlayerPropertyChangedHandler;

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
                _privateDeckChoiceWindow.Template = Resources["DarkGreenWindowStyle"] as ControlTemplate;
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
            profileBoxes.Clear();
            playersMap.Clear();
            GameViewModel model = GameModel;
            for (int i = 1; i < model.PlayerModels.Count; i++)
            {
                _CreatePlayerInfoView(i);
            }                        
            playersMap.Add(model.MainPlayerModel.Player, mainPlayerPanel);
            RearrangeSeats();
            _Resize(new Size(this.ActualWidth, this.ActualHeight));
            model.PropertyChanged += new PropertyChangedEventHandler(model_PropertyChanged);
            model.Game.PropertyChanged += new PropertyChangedEventHandler(_game_PropertyChanged);
            Trace.Assert(model.MainPlayerModel != null, "Main player must exist.");
            
            var oldModel = e.OldValue as GameViewModel;
            if (oldModel != null)
            {
                Trace.Assert(oldModel.MainPlayerModel != null, "Main player must exist.");
                oldModel.PropertyChanged -= _mainPlayerPropertyChangedHandler;
            }
            model.MainPlayerModel.PropertyChanged += _mainPlayerPropertyChangedHandler;

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
                model.PlayerModels[i].PropertyChanged += new PropertyChangedEventHandler(_player_PropertyChanged);                
            }       
        }

        ChildWindow cardChoiceWindow;
        void _player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                PlayerViewModel model = sender as PlayerViewModel;
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
                        cardChoiceWindow.Template = Resources["DarkGreenWindowStyle"] as ControlTemplate;
                        cardChoiceWindow.MaxWidth = 800;
                        cardChoiceWindow.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                        cardChoiceWindow.CloseButtonVisibility = model.CardChoiceModel.CanClose ? Visibility.Visible : Visibility.Collapsed;
                        cardChoiceWindow.Effect = new DropShadowEffect(){ BlurRadius = 10d };
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
            });
        }       

        void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MainPlayerSeatNumber")
            {
                RearrangeSeats();
            }
        }

        void _game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
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

            mainPlayerPanel.DataContext = model.MainPlayerModel;
            playersMap[model.MainPlayerModel.Player] = mainPlayerPanel;
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
            Storyboard lineUpGroup = new Storyboard();
            List<Line> lines = new List<Line>();
            var src = playersMap[source];
            Point srcPoint = src.TranslatePoint(new Point(src.ActualWidth / 2, src.ActualHeight / 2), GlobalCanvas);
            foreach (var target in targets)
            {
                var dest = playersMap[target];
                Point dstPoint = dest.TranslatePoint(new Point(dest.ActualWidth / 2, dest.ActualHeight / 2), GlobalCanvas);
                double distance = Math.Sqrt((srcPoint.X - dstPoint.X) * (srcPoint.X - dstPoint.X) + (srcPoint.Y - dstPoint.Y) * (srcPoint.Y - dstPoint.Y)); 
                
                Line line = new Line();
                line.Stroke = Resources["indicatorLineBrush"] as Brush;
                line.X1 = srcPoint.X;
                line.X2 = dstPoint.X;
                line.Y1 = srcPoint.Y;
                line.Y2 = dstPoint.Y;
                line.StrokeThickness = 1;                
                lines.Add(line);

                Line line2 = new Line();
                line2.Stroke = Resources["indicatorLineGlowBrush"] as Brush;
                line2.X1 = srcPoint.X;
                line2.X2 = dstPoint.X;
                line2.Y1 = srcPoint.Y;
                line2.Y2 = dstPoint.Y;
                line2.StrokeThickness = 3;
                lines.Add(line2);
            }

            foreach (var line in lines)
            {
                double distance = Math.Sqrt((line.X2 - line.X1) * (line.X2 - line.X1) + (line.Y2 - line.Y1) * (line.Y2 - line.Y1));
                line.StrokeDashArray = new DoubleCollection() { distance * 2.0, 10000d };
                line.StrokeDashOffset = distance;
                line.StrokeDashCap = PenLineCap.Triangle;

                DoubleAnimation animation = new DoubleAnimation(distance * 2.0, -distance, _lineUpDuration);
                Storyboard.SetTarget(animation, line);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Line.StrokeDashOffsetProperty));
                lineUpGroup.Children.Add(animation);
                GlobalCanvas.Children.Add(line);
                animation.Completed += (o, e) =>
                {
                    var da = (o as AnimationClock).Timeline;
                    Line l = Storyboard.GetTarget(da) as Line;
                    GlobalCanvas.Children.Remove(l);
                };
            }
            lineUpGroup.AccelerationRatio = 0.6;
            lineUpGroup.Begin();
        }

        
        #endregion

        #region INotificationProxy

        public void NotifyCardMovement(List<CardsMovement> moves)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                bool doWeaponSound = false;
                bool doArmorSound = false;
                bool doHorseSound = false;
                foreach (CardsMovement move in moves)
                {
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
                        IList<CardView> cards;
                        Trace.Assert(move.Helper != null);
                        if (!move.Helper.IsFakedMove)
                        {
                            gameLogs.AppendCardMoveLog(stackCards.Value, stackCards.Key, move.To);
                        }

                        cards = deck.RemoveCards(stackCards.Key.DeckType, stackCards.Value);
                        cardsToAdd.AddRange(cards);
                    }

                    _GetMovementDeck(move.To).AddCards(move.To.DeckType, cardsToAdd, move.Helper.IsFakedMove);
                }
                rtbLog.ScrollToEnd();
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

            }, System.Windows.Threading.DispatcherPriority.Send);
        }

        public void NotifyDamage(Player source, Player target, int magnitude, DamageElement element)
        {
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

        public void NotifySkillUse(ActionLog log)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                Trace.Assert(log.Source != null);
                PlayerViewBase player = playersMap[log.Source];
                if (log.SkillAction != null)
                {
                    string key1 = string.Format("{0}.Animation", log.SkillAction.GetType().Name);
                    string key2 = key1 + ".Offset";
                    bool animPlayed = false;
                    lock (equipAnimationResources)
                    {
                        if (equipAnimationResources.Contains(key1))
                        {
                            AnimationBase animation = equipAnimationResources[key1] as AnimationBase;
                            if (animation != null && animation.Parent == null)
                            {
                                Point offset = new Point(0, 0);
                                if (equipAnimationResources.Contains(key2))
                                {
                                    offset = (Point)equipAnimationResources[key2];                                    
                                }
                                player.PlayAnimation(animation, 0, offset);
                                animPlayed = true;
                            }
                        }
                    }
                    if (log.SkillAction.IsSingleUse || log.SkillAction.IsAwakening)
                    {
                        ExcitingSkillAnimation anim = new ExcitingSkillAnimation();
                        anim.SkillName = log.SkillAction.GetType().Name;
                        anim.HeroName = log.Source.Hero.Name;
                        gridRoot.Children.Add(anim);
                        anim.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        anim.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        anim.Start();
                        animPlayed = true;
                    }
                    if (!animPlayed && player != mainPlayerPanel)
                    {
                        string s = LogFormatter.Translate(log.SkillAction);
                        if (s != string.Empty)
                        {
                            
                            ZoomTextAnimation anim = new ZoomTextAnimation() { Text = s };
                            player.PlayAnimation(anim, 1, new Point(0, 0));
                            animPlayed = true;                            
                        }
                    }
                    string soundKey = log.SkillAction.GetType().Name;
                    Uri uri = GameSoundLocator.GetSkillSound(soundKey, log.SpecialEffectHint);
                    GameSoundPlayer.PlaySoundEffect(uri);
                }
                if (log.CardAction != null)
                {
                    if (log.CardAction.Type is Shan)
                    {                        
                        player.PlayAnimation(new ShanAnimation(), 0, new Point(0, 0));
                    }
                    else if (log.CardAction.Type is RegularSha)
                    {
                        AnimationBase sha;
                        if (log.CardAction.SuitColor == SuitColorType.Red)
                        {
                            sha = new ShaAnimation();
                        }
                        else
                        {
                            sha = new ShaAnimation2();
                        }
                        player.PlayAnimation(sha, 0, new Point(0, 0));
                    }
                    else if (log.CardAction.Type is TieSuoLianHuan)
                    {
                        foreach (var p in log.Targets)
                        {
                            playersMap[p].PlayIronShackleAnimation();
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
                        if (play) GameSoundPlayer.PlaySoundEffect(cardSoundUri);
                    }
                }

                if (log.Targets.Count > 0)
                {
                    _LineUp(log.Source, log.Targets);
                    foreach (var target in log.Targets)
                    {
                        target.IsTargeted = true;
                    }
                }

                gameLogs.AppendLog(log);
                rtbLog.ScrollToEnd();

                _AppendKeyEventLog(log);
            });
        }

        public void NotifyLogEvent(Prompt prompt)
        {
        }

        private ChildWindow _showHandCardsWindow;

        public void NotifyShowCardsStart(Player p, List<Card> cards)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                if (_showHandCardsWindow != null)
                {
                    gridRoot.Children.Remove(_showHandCardsWindow);
                }

                _showHandCardsWindow = new ChildWindow();
                _showHandCardsWindow.Template = Resources["DarkGreenWindowStyle"] as ControlTemplate;
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
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                if (_showHandCardsWindow == null) return;
                gridRoot.Children.Remove(_showHandCardsWindow);
                _showHandCardsWindow = null;
            });
        }

        private void _AppendKeyEventLog(Paragraph log)
        {
            var doc = new FlowDocument();
            var para = log;
            if (para.Inlines.Count == 0) return;
            doc.Blocks.Add(para);
            keyEventLog.AddLog(doc);
        }

        private void _AppendKeyEventLog(ActionLog log)
        {
            _AppendKeyEventLog(LogFormatter.RichTranslateKeyLog(log));
        }


        public void NotifyMultipleChoiceResult(Player p, OptionPrompt answer)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                gameLogs.AppendMultipleChoiceLog(p, PromptFormatter.Format(answer));
            });
        }

        public void NotifyDeath(Player p, Player by)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                var uri = GameSoundLocator.GetDeathSound(p.Hero.Name);
                GameSoundPlayer.PlaySoundEffect(uri);
                gameLogs.AppendDeathLog(p, by);
            });
        }

        public void NotifyActionComplete()
        {
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
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                gameLogs.AppendLoseHealthLog(player, delta);
                rtbLog.ScrollToEnd();
                Uri uri = GameSoundLocator.GetSystemSound("LoseHealth");
                GameSoundPlayer.PlaySoundEffect(uri);
            });
        }

        public void NotifyRecoverHealth(Player player, int delta)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                gameLogs.AppendRecoverHealthLog(player, delta);
                rtbLog.ScrollToEnd();
                Uri uri = GameSoundLocator.GetSystemSound("RecoverHealth");
                GameSoundPlayer.PlaySoundEffect(uri);
            });
        }

        public void NotifyShowCard(Player p, Card card)
        {
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
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                if (cardChoiceWindow == null) return;
                var box = cardChoiceWindow.Content as CardArrangeBox;
                if (box == null) return;
                box.MoveCard(arrange);
            });
        }

        public void NotifyImpersonation(Player p, Core.Heroes.Hero h, ISkill s)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                var model = playersMap[p].PlayerModel;
                if (h == null)
                {
                    model.ImpersonatedHeroName = string.Empty;
                    model.ImpersonatedSkill = string.Empty;
                }
                else
                {
                    model.ImpersonatedHeroName = h.Name;
                    model.ImpersonatedSkill = s.GetType().Name;
                }
            });
        }

        public void NotifyGameStart()
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                GameSoundPlayer.PlaySoundEffect(GameSoundLocator.GetSystemSound("GameStart"));
            });
        }

        public void NotifyJudge(Player p, Card card, ActionLog log, bool? isSuccess, bool isFinalResult)
        {
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


        public void NotifyWuGuStart(DeckPlace place)
        {
        }

        public void NotifyWuGuEnd()
        {
        }


        public void NotifyPinDianStart(Player from, Player to, ISkill reason)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                pinDianWindow.Caption = PromptFormatter.Format(new Prompt("Window.PinDian.Prompt", reason));
                pinDianBox.StartPinDian(from, to);
                pinDianWindow.Show();
            });
        }

        public void NotifyMultipleCardUsageResponded(Player player)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                pinDianBox.OnPinDianCardPlayed(player);
            });
        }

        public void NotifyPinDianEnd(Card c1, Card c2)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {                
                pinDianBox.RevealResult(c1, c2);
            });
        }

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
                ObservableCollection<GameResultViewModel> model = new ObservableCollection<GameResultViewModel>();
                foreach (Player player in winners.Concat(losers).Concat(drawers))
                {
                    var m = new GameResultViewModel();
                    m.Player = player;
                    if (winners.Contains(player))
                    {
                        m.Result = GameResult.Win;
                        // @todo : fix this.
                        m.GainedExperience = "+15";
                        m.GainedTechPoints = "+0";
                    }
                    else if (losers.Contains(player))
                    {
                        m.Result = GameResult.Lose;
                        // @todo : fix this.
                        m.GainedExperience = "-3";
                        m.GainedTechPoints = "+0";
                    }
                    else if (drawers.Contains(player))
                    {
                        m.Result = GameResult.Draw;
                        // @todo : fix this.
                        m.GainedExperience = "+3";
                        m.GainedTechPoints = "+0";
                    }
                    model.Add(m);
                }
                LobbyViewModel.Instance.OnChat -= chatEventHandler;
                gameResultBox.DataContext = model;
                gameResultWindow.Content = gameResultBox;
                gameResultWindow.Show();
                gameResultWindow.Closed += (o, e) =>
                {
                    var handler = OnGameCompleted;
                    if (handler != null)
                    {
                        handler(this, new EventArgs());
                    }
                };
            });
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GameSoundPlayer.PlayBackgroundMusic(GameSoundLocator.GetBgm());
        }        
    }
}
