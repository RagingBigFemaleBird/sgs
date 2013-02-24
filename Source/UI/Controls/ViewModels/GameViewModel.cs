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
            PlayerModels = new ObservableCollection<PlayerViewModel>();
        }

        public ObservableCollection<PlayerViewModel> PlayerModels
        {
            get;
            set;
        }

        public Game Game
        {
            get { return _game; }
            set 
            {
                if (_game == value) return;
                _game = value;
                if (_game == null) return;
                PlayerModels.Clear();
                int i = 0;
                foreach (var player in _game.Players)
                {
                    Trace.Assert(_game.Settings.Accounts.Count == _game.Players.Count);
                    PlayerModels.Add(new PlayerViewModel(player, this) 
                    {
                        Account = _game.Settings.Accounts[i] 
                    });
                    i++;
                }
                if (_game.ReplayController != null)
                {
                    ReplayController = new ReplayControllerViewModel(_game.ReplayController);                    
                }
            }
        }        
        
        public PlayerViewModel MainPlayerModel
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
                int gameSeat = (i + MainPlayerSeatNumber) % playerCount;
                Player gamePlayer = _game.Players[gameSeat];
                bool found = false;
                for (int j = i; j < playerCount; j++)
                {                    
                    PlayerViewModel playerModel = PlayerModels[j];
                    if (gamePlayer == playerModel.Player)
                    {
                        if (j != i)
                        {
                            PlayerModels.Move(j, i);
                        }
                        found = true;
                        break;
                    }                    
                }
                Trace.Assert(found);
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
                OnPropertyChanged("MainPlayerModel");
            }
        }

        private WuGuChoiceViewModel _wuGuModel;

        public WuGuChoiceViewModel WuGuModel
        {
            get
            {
                return _wuGuModel;
            }
            set
            {
                if (_wuGuModel == value) return;
                _wuGuModel = value;
                OnPropertyChanged("WuGuModel");
            }
        }

        public PlayerViewModel CurrentActivePlayer
        {
            get;
            set;
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

        private ReplayControllerViewModel _replayController;

        public ReplayControllerViewModel ReplayController
        {
            get
            {
                return _replayController;
            }
            private set
            {
                if (_replayController == value) return;
                _replayController = value;
                OnPropertyChanged("ReplayController");
            }
        }

        public bool IsReplay
        {
            get
            {
                if (_game == null) return false;
                return _game.ReplayController != null;
            }
        }

        public bool IsSpectating
        {
            get
            {
                if (_game == null) return false;
                if (_game.GameClient == null) return false;
                return _game.GameClient.SelfId >= _game.Players.Count;
            }
        }

        public bool IsPlayable
        {
            get
            {
                return !IsReplay && !IsSpectating;
            }
        }
    }
}
