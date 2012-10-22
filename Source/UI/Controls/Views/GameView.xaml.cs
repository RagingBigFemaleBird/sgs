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

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for GameTable.xaml
    /// </summary>
    public partial class GameView : UserControl, IAsyncUiProxy
    {
        #region Private Members
        protected static int[][] regularSeatIndex;
        protected static int[][] pk3v3SeatIndex;
        protected static int[][] pk1v3SeatIndex;
        private IList<StackPanel> stackPanels;
        private IList<PlayerInfoView> profileBoxes;
        private IList<PlayerInfoViewModel> playerModels;
        private IDictionary<Player, PlayerInfoViewBase> playersMap;
        private PlayerInfoViewModel mainPlayerModel;
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
            stackPanels = new List<StackPanel>() { stackPanel0, stackPanel1, stackPanel2, stackPanel3, stackPanel4, stackPanel5 };
            profileBoxes = new List<PlayerInfoView>();
            playersMap = new Dictionary<Player, PlayerInfoViewBase>();
            playerModels = new List<PlayerInfoViewModel>();
            mainPlayerPanel.ParentGameView = this;
            discardDeck.ParentGameView = this;
            this.DataContextChanged +=  new DependencyPropertyChangedEventHandler(GameView_DataContextChanged);
            _UpdateEnabledStatusHandler = 
                (o, e) => {_UpdateCommandStatus();};
            this.SizeChanged += new SizeChangedEventHandler(GameView_SizeChanged);
            CardUsageAnsweredEvent += new CardUsageAnsweredEventHandler(GameView_CardUsageAnsweredEvent);
            _timer = new System.Timers.Timer();
        }

        void GameView_CardUsageAnsweredEvent(ISkill skill, List<Card> cards, List<Player> players)
        {
            mainPlayerModel.TimeOutSeconds = 0;
        }

        void GameView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (profileBoxes.Count == 0)
            {
                return;
            }

            Size tableBounds = new Size();
            tableBounds.Width = e.NewSize.Width - gridRoot.ColumnDefinitions[1].Width.Value;
            tableBounds.Height = e.NewSize.Height - gridRoot.RowDefinitions[1].Height.Value -
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

            width = Math.Min(width, height * ratio);
            height = width / ratio;

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
            if (e.NewSize.Width > thresh)
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
            discardDeck.RearrangeCards(0d);
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

        #region Private Functions

        private void GameView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RearrangeSeats();
        }

        #endregion
        
        #region Layout Related
        public void RearrangeSeats()
        {
            GameViewModel model = DataContext as GameViewModel;

            int[] seatMap = null;

            int numPlayers = model.Game.Players.Count;

            if (numPlayers == 0) return;

            if (numPlayers >= 10)
            {
                throw new NotImplementedException("Only fewer than 10 players are supported by UI.");
            }

            profileBoxes.Clear();
            playersMap.Clear();
            playerModels.Clear();
            var players = model.Game.Players;
            for (int i = 1; i < players.Count; i++)
            {
                int seatNo = (model.MainPlayerSeatNumber + i) % players.Count;
                PlayerInfoViewModel playerModel = new PlayerInfoViewModel();
                playerModel.Player = players[seatNo];
                playerModel.Game = model.Game;
                var playerView = new PlayerInfoView() { DataContext = playerModel, ParentGameView = this };
                profileBoxes.Add(playerView);
                playerModels.Add(playerModel);
                playersMap.Add(players[seatNo], playerView);
            }

            Player self = players[model.MainPlayerSeatNumber];
            mainPlayerModel = new PlayerInfoViewModel();
            mainPlayerModel.SubmitAnswerCommand = submitCommand = new SimpleRelayCommand(ExecuteSubmitCommand);
            mainPlayerModel.CancelAnswerCommand = cancelCommand = new SimpleRelayCommand(ExecuteCancelCommand);
            mainPlayerModel.AbortAnswerCommand = abortCommand = new SimpleRelayCommand(ExecuteAbortCommand);
            mainPlayerModel.Game = model.Game;
            mainPlayerModel.Player = self;
            mainPlayerPanel.DataContext = mainPlayerModel;
            playerModels.Add(mainPlayerModel);
            playersMap.Add(self, mainPlayerPanel);

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
                        PlayerInfoView view = box as PlayerInfoView;
                        view.Margin = new Thickness(0, vSpacing / 2, 0, vSpacing / 2);
                    }
                }
                else
                {
                    foreach (var box in panel.Children)
                    {
                        PlayerInfoView view = box as PlayerInfoView;
                        view.Margin = new Thickness(hSpacing / 2, 0, hSpacing / 2, 0);
                    }
                }
            }
        }
        
        private static int _minHSpacing = 10;
        private static int _minVSpacing = 2;
        #endregion

        #region Card Movement

        private IDeckContainer _GetMovementDeck(DeckPlace place)
        {
            if (place.Player != null)
            {
                PlayerInfoViewBase playerView = playersMap[place.Player];
                return playerView;                
            }
            else
            {
                return discardDeck;
            }
        }

        #endregion

        #region Helpers


        SkillCommand _GetSelectedSkillCommand(out bool isEquipSkill)
        {
            foreach (var skillCommand in mainPlayerModel.SkillCommands)
            {                
                if (skillCommand.IsSelected)
                {
                    isEquipSkill = false;
                    return skillCommand;
                }
            }
            foreach (EquipCommand equipCmd in mainPlayerModel.EquipCommands)
            {
                if (equipCmd.IsSelected)
                {
                    isEquipSkill = true;
                    return equipCmd.SkillCommand;
                }
            }
            isEquipSkill = false;
            return null;
        }

        private List<Card> _GetSelectedHandCards()
        {
            List<Card> cards = new List<Card>();
            foreach (CardView cardView in mainPlayerPanel.HandCardArea.Cards)
            {
                if (cardView.CardViewModel.IsSelected)
                {
                    Trace.Assert(cardView.Card != null);
                    cards.Add(cardView.Card);
                }
            }
            return cards;
        }

        private List<Player> _GetSelectedPlayers()
        {
            List<Player> players = new List<Player>();
            foreach (var playerModel in playerModels)
            {
                if (playerModel.IsSelected)               
                {
                    players.Add(playerModel.Player);
                }
            }
            return players;
        }

        private void _ResetButtons()
        {
            foreach (var equipCommand in mainPlayerModel.EquipCommands)
            {
                equipCommand.OnSelectedChanged -= _UpdateEnabledStatusHandler;
                equipCommand.IsSelectionMode = false;
            }

            foreach (var skillCommand in mainPlayerModel.SkillCommands)
            {
                skillCommand.IsSelected = false;
                skillCommand.IsEnabled = false;
            }

            foreach (CardView cardView in mainPlayerPanel.HandCardArea.Cards)
            {
                cardView.CardViewModel.OnSelectedChanged -= _UpdateEnabledStatusHandler;
                cardView.CardViewModel.IsSelectionMode = false;
            }

            foreach (var playerModel in playerModels)
            {
                playerModel.OnSelectedChanged -= _UpdateEnabledStatusHandler;
                playerModel.IsSelectionMode = false;
            }

            GameModel.CurrentPrompt = string.Empty;

            submitCommand.CanExecuteStatus = false;
            cancelCommand.CanExecuteStatus = false;
            abortCommand.CanExecuteStatus = false;
        }

        private void _ResetAll()
        {
            GameModel.MultiChoiceCommands.Clear();
            _ResetButtons();
        }

        #endregion

        #region Commands

        #region SubmitAnswerCommand

        SimpleRelayCommand submitCommand;

        public void ExecuteSubmitCommand(object parameter)
        {
            List<Card> cards = _GetSelectedHandCards();
            List<Player> players = _GetSelectedPlayers();
            ISkill skill = null;
            bool isEquipSkill;
            SkillCommand skillCommand = _GetSelectedSkillCommand(out isEquipSkill);

            foreach (var equipCommand in mainPlayerModel.EquipCommands)
            {
                if (!isEquipSkill && equipCommand.IsSelected)
                {
                    cards.Add(equipCommand.Card);
                }
            }
            
            if (skillCommand != null)
            {
                skill = skillCommand.Skill;
            }

            _ResetAll();

            // Card usage question
            if (currentUsageVerifier != null)
            {
                currentUsageVerifier = null;
                CardUsageAnsweredEvent(skill, cards, players);
            }
        }

        #endregion

        #region CancelAnswerCommand

        SimpleRelayCommand cancelCommand;

        public void ExecuteCancelCommand(object parameter)
        {
            if (currentUsageVerifier != null)
            {
                // @todo
                //if (currentUsageVerifier.GetType().Name!="PlayerActionStageVerifier")
                {
                    CardUsageAnsweredEvent(null, null, null);
                    currentUsageVerifier = null;
                    _ResetAll();
                }
                /*else
                {
                    _ResetAll();
                }*/
            }            
        }
        #endregion

        #region AbortAnswerCommand

        SimpleRelayCommand abortCommand;

        public void ExecuteAbortCommand(object parameter)
        {
            _timer.Stop();
            if (currentUsageVerifier != null)
            {
                CardUsageAnsweredEvent(null, null, null);
                currentUsageVerifier = null;
                _ResetAll();
            }
        }
        #endregion

        #region MultiChoiceCommand
        public void ExecuteMultiChoiceCommand(object parameter)
        {
            _ResetAll();
            MultipleChoiceAnsweredEvent((int)parameter);
        }
        #endregion

        #endregion

        #region IAsyncUiProxy
        public Player HostPlayer
        {
            get
            {
                PlayerInfoViewModel hostPlayer = mainPlayerPanel.DataContext as PlayerInfoViewModel;
                if (hostPlayer == null) return null;
                return hostPlayer.Player;
            }
            set
            {
                PlayerInfoViewModel hostPlayer = mainPlayerPanel.DataContext as PlayerInfoViewModel;
                GameViewModel gameModel = DataContext as GameViewModel;
                Trace.Assert(gameModel != null && gameModel.Game != null);
                var players = gameModel.Game.Players;
                bool found = false;
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i] == hostPlayer.Player)
                    {
                        gameModel.MainPlayerSeatNumber = i;
                        found = true;
                        break;
                    }
                }
                Trace.Assert(found);
            }
        }

        public void NotifyCardMovement(List<CardsMovement> moves, List<IGameLog> notes)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                foreach (CardsMovement move in moves)
                {
                    var cardsToAdd = new List<CardView>();
                    var cardsRemoved = new Dictionary<DeckPlace, List<Card>>();
                    foreach (Card card in move.cards)
                    {
                        if (!cardsRemoved.ContainsKey(card.Place))
                        {
                            cardsRemoved.Add(card.Place, new List<Card>());
                        }
                        cardsRemoved[card.Place].Add(card);
                    }
                    foreach (var stackCards in cardsRemoved)
                    {
                        IDeckContainer deck = _GetMovementDeck(stackCards.Key);
                        IList<CardView> cards;
                        cards = deck.RemoveCards(stackCards.Key.DeckType, stackCards.Value);
                        cardsToAdd.AddRange(cards);
                    }
                    _GetMovementDeck(move.to).AddCards(move.to.DeckType, cardsToAdd);
                }
            });            
        }

        ICardUsageVerifier currentUsageVerifier;
        bool isMultiChoiceQuestion;

        private void _UpdateCardUsageStatus()
        {
            List<Card> cards = _GetSelectedHandCards();
            List<Player> players = _GetSelectedPlayers();
            ISkill skill = null;
            bool isEquipCommand;
            SkillCommand command = _GetSelectedSkillCommand(out isEquipCommand);

            if (command != null)
            {
                skill = command.Skill;
            }

            // Handle skill down            
            foreach (var skillCommand in mainPlayerModel.SkillCommands)
            {
                skillCommand.IsEnabled = (currentUsageVerifier.Verify(skillCommand.Skill, new List<Card>(), new List<Player>()) != VerifierResult.Fail);
            }

            if (skill == null)
            {
                foreach (var equipCommand in mainPlayerModel.EquipCommands)
                {
                    if (equipCommand.SkillCommand.Skill == null)
                    {
                        equipCommand.IsEnabled = false;
                    }
                    equipCommand.IsEnabled = (currentUsageVerifier.Verify(equipCommand.SkillCommand.Skill, new List<Card>(), new List<Player>()) != VerifierResult.Fail);
                }
            }
            if (!isEquipCommand)
            {
                foreach (var equipCommand in mainPlayerModel.EquipCommands)
                {
                    if (equipCommand.IsSelected)
                        cards.Add(equipCommand.Card);
                }
            }

            var status = currentUsageVerifier.Verify(skill, cards, players);

            if (status == VerifierResult.Fail)
            {
                submitCommand.CanExecuteStatus = false;
                foreach (var playerModel in playerModels)
                {
                    playerModel.IsSelected = false;
                }
            }
            else if (status == VerifierResult.Partial)
            {
                submitCommand.CanExecuteStatus = false;
            }
            else if (status == VerifierResult.Success)
            {
                submitCommand.CanExecuteStatus = true;
            }

            List<Card> attempt = new List<Card>(cards);

            foreach (CardView cardView in mainPlayerPanel.HandCardArea.Cards)
            {
                if (cardView.CardViewModel.IsSelected)
                {
                    continue;
                }
                attempt.Add(cardView.Card);
                bool disabled = (currentUsageVerifier.Verify(skill, attempt, players) == VerifierResult.Fail);
                cardView.CardViewModel.IsEnabled = !disabled;
                attempt.Remove(cardView.Card);
            }

            if (skill != null)
            {
                foreach (var equipCommand in mainPlayerModel.EquipCommands)
                {
                    if (equipCommand.IsSelected) continue;

                    attempt.Add(equipCommand.Card);
                    bool disabled = (currentUsageVerifier.Verify(skill, attempt, players) == VerifierResult.Fail);
                    equipCommand.IsEnabled = !disabled;
                    attempt.Remove(equipCommand.Card);
                }
            }

            // Invalidate target selection
            List<Player> attempt2 = new List<Player>(players);
            int validCount = 0;
            bool[] enabledMap = new bool[playerModels.Count];
            int i = 0;
            foreach (var playerModel in playerModels)
            {
                enabledMap[i] = false;
                if (playerModel.IsSelected)
                {
                    i++;
                    continue;
                }
                attempt2.Add(playerModel.Player);
                bool disabled = (currentUsageVerifier.Verify(skill, cards, attempt2) == VerifierResult.Fail);
                if (!disabled)
                {
                    validCount++;
                    enabledMap[i] = true;
                }
                attempt2.Remove(playerModel.Player);
                i++;

            }
            i = 0;

            bool allowSelection = (cards.Count != 0 || validCount != 0 || skill != null);
            foreach (var playerModel in playerModels)
            {
                if (playerModel.IsSelected)
                {
                    i++;
                    continue;
                }
                playerModel.IsSelectionMode = allowSelection;
                if (allowSelection)
                {
                    playerModel.IsEnabled = enabledMap[i];
                }
                i++;
            }
        }

        private void _UpdateCommandStatus()
        {
            if (currentUsageVerifier != null)
            {
                _UpdateCardUsageStatus();
            }
            else if (isMultiChoiceQuestion)
            {
                _ResetButtons();
            }
        }

        private System.Timers.Timer _timer;

        public void AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, int timeOutSeconds)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                currentUsageVerifier = verifier;
                Game.CurrentGame.CurrentActingPlayer = HostPlayer;
                GameModel.CurrentPrompt = PromptFormatter.Format(prompt);

                foreach (var equipCommand in mainPlayerModel.EquipCommands)
                {
                    equipCommand.OnSelectedChanged += _UpdateEnabledStatusHandler;
                    equipCommand.IsSelectionMode = true;
                }

                foreach (CardView cardView in mainPlayerPanel.HandCardArea.Cards)
                {
                    cardView.CardViewModel.IsSelectionMode = true;
                    cardView.CardViewModel.OnSelectedChanged += _UpdateEnabledStatusHandler;
                }
                
                foreach (var playerModel in playerModels)
                {
                    playerModel.IsSelectionMode = true;
                    playerModel.OnSelectedChanged += _UpdateEnabledStatusHandler;
                }

                foreach (var skillCommand in mainPlayerModel.SkillCommands)
                {
                    skillCommand.OnSelectedChanged += _UpdateEnabledStatusHandler;
                }

                // @todo: update this.
                cancelCommand.CanExecuteStatus = true;
                abortCommand.CanExecuteStatus = true;

                if (timeOutSeconds > 0)
                {
                    mainPlayerModel.TimeOutSeconds = timeOutSeconds;

                    _timer = new System.Timers.Timer(timeOutSeconds * 1000);
                    _timer.AutoReset = false;
                    _timer.Elapsed +=
                        (o, e) =>
                        {
                            Application.Current.Dispatcher.Invoke(
                                (ThreadStart)delegate() { ExecuteAbortCommand(null); });
                        };
                    _timer.Start();
                }
                _UpdateCommandStatus();
            });
        }

        private EventHandler _UpdateEnabledStatusHandler;

        public void AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, int timeOutSeconds)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                GameModel.CurrentPrompt = PromptFormatter.Format(prompt);
                CardChoiceAnsweredEvent(null);
            });
        }

        public void AskForMultipleChoice(Prompt prompt, List<string> choices, int timeOutSeconds)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                GameModel.CurrentPrompt = PromptFormatter.Format(prompt);                
                for (int i = 0; i < choices.Count; i++)
                {
                    GameModel.MultiChoiceCommands.Add(
                        new MultiChoiceCommand(ExecuteMultiChoiceCommand) 
                        {
                            CanExecuteStatus = true, 
                            ChoiceKey = choices[i], 
                            ChoiceIndex = i 
                        });
                }
                isMultiChoiceQuestion = true;
                _UpdateCommandStatus();
            });
        }

        public int TimeOutSeconds
        {
            get;
            set;
        }

        public event CardUsageAnsweredEventHandler CardUsageAnsweredEvent;

        public event CardChoiceAnsweredEventHandler CardChoiceAnsweredEvent;

        public event MultipleChoiceAnsweredEventHandler MultipleChoiceAnsweredEvent;

        #endregion        
    }
}
