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

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for GameTable.xaml
    /// </summary>
    public partial class GameView : UserControl, IUiProxy
    {
        #region Private Members
        protected static int[][] regularSeatIndex;
        protected static int[][] pk3v3SeatIndex;
        protected static int[][] pk1v3SeatIndex;
        private IList<StackPanel> stackPanels;
        private IList<PlayerInfoView> profileBoxes;
        private IDictionary<Player, PlayerInfoViewBase> playersMap;
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
            playersMap = new Dictionary<Player, PlayerInfoView>();
            this.DataContextChanged +=  new DependencyPropertyChangedEventHandler(GameView_DataContextChanged);
            
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
            var players = model.Game.Players;
            for (int i = 1; i < players.Count; i++)
            {
                int seatNo = (model.MainPlayerSeatNumber + i) % players.Count;
                PlayerInfoViewModel playerModel = new PlayerInfoViewModel();
                playerModel.Player = players[seatNo];
                playerModel.Game = model.Game;
                var playerView = new PlayerInfoView() { DataContext = playerModel, ParentGameView = this };
                profileBoxes.Add(playerView);
                playersMap.Add(players[seatNo], playerView);
            }

            Player self = players[model.MainPlayerSeatNumber];
            PlayerInfoViewModel mainPlayerModel = new PlayerInfoViewModel();
            mainPlayerModel.Game = model.Game;
            mainPlayerModel.Player = self;
            mainPlayerPanel.DataContext = mainPlayerModel;
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
        
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (profileBoxes.Count == 0)
            {
                return base.ArrangeOverride(arrangeBounds);
            }

            Size tableBounds = new Size();
            tableBounds.Width = arrangeBounds.Width - gridRoot.ColumnDefinitions[1].Width.Value;
            tableBounds.Height = arrangeBounds.Height - gridRoot.RowDefinitions[1].Height.Value -
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
            if (arrangeBounds.Width > thresh)
            {
                mainPlayerPanel.SetValue(Grid.ColumnSpanProperty, 1);
                infoPanel.SetValue(Grid.RowSpanProperty, 2);
            }
            else
            {
                mainPlayerPanel.SetValue(Grid.ColumnSpanProperty, 2);
                infoPanel.SetValue(Grid.RowSpanProperty, 1);
            }

            base.ArrangeOverride(arrangeBounds);

            return arrangeBounds;
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

        private CardView _CreateCard(Card card)
        {
            return new CardView() { DataContext = new CardViewModel() { Card = card } };
        }

        private IList<CardView> _CreateCard(IList<Card> cards)
        {
            List<CardView> cardViews = new List<CardView>();
            foreach (Card card in cards)
            {
                cardViews.Add(_CreateCard(card));
            }
            return cardViews;
        }

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

        #region IUiProxy
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

        public bool AskForCardUsage(string prompt, ICardUsageVerifier verifier, out Core.Skills.ISkill skill, out List<Card> cards, out List<Player> players)
        {
            throw new NotImplementedException();
        }

        public bool AskForCardChoice(string prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer)
        {
            throw new NotImplementedException();
        }

        public void NotifyUiLog(List<CardsMovement> moves, List<IGameLog> notes)
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
                    cardsToAdd.AddRange(deck.RemoveCards(stackCards.Key.DeckType, stackCards.Value));
                }
                _GetMovementDeck(move.to).AddCards(move.to.DeckType, cardsToAdd);
            }
        }
        #endregion
    }
}
