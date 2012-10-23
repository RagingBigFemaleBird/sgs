using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;

namespace Sanguosha.UI.Controls
{
    public enum GameTableLayout
    {
        Regular,
        Pk3v3,
        Pk1v3
    }

    public class GameViewModel : ViewModelBase
    {
        private Game _game;

        public GameViewModel()
        {
            PlayerModels = new ObservableCollection<PlayerInfoViewModel>();
        }

        public ObservableCollection<PlayerInfoViewModel> PlayerModels
        {
            get;
            set;
        }

        public Game Game
        {
            get { return _game; }
            set 
            {
                _game = value;
                _game.RegisterCurrentThread();
                PlayerModels.Clear();
                foreach (var player in _game.Players)
                {
                    PlayerModels.Add(new PlayerInfoViewModel(player));
                }
            }
        }

        public PlayerInfoViewModel MainPlayerModel
        {
            get
            {                
                return PlayerModels[0];
            }
        }

        private void _RearrangeSeats()
        {
            Trace.Assert(_game.Players.Count == PlayerModels.Count);
            int playerCount = _game.Players.Count;
            for (int i = 0; i < playerCount; i++)
            {
                Player gamePlayer = _game.Players[i];
                for (int j = i; j < playerCount + MainPlayerSeatNumber; j++)
                {
                    int currentSeat = j % playerCount;
                    PlayerInfoViewModel playerModel = PlayerModels[currentSeat];
                    if (gamePlayer == playerModel.Player)
                    {
                        if (i != currentSeat)
                        {
                            PlayerModels.Move(currentSeat, i);
                        }
                        break;
                    }
                    Trace.Assert(false);
                }
            }
        }

        int _mainPlayerSeatNumber;

        public int MainPlayerSeatNumber
        {
            get { return _mainPlayerSeatNumber; }
            set 
            {
                if (_mainPlayerSeatNumber == value) return;
                _mainPlayerSeatNumber = value;
                _RearrangeSeats();
                OnPropertyChanged("MainPlayerSeatNumber");
            }
        }

        public GameTableLayout TableLayout
        {
            get 
            {
                if (_game is RoleGame)
                {
                    return GameTableLayout.Regular;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
