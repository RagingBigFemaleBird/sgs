using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

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

        public Game Game
        {
            get { return _game; }
            set { _game = value; }
        }

        int _mainPlayerSeatNumber;

        public int MainPlayerSeatNumber
        {
            get { return _mainPlayerSeatNumber; }
            set 
            {
                if (_mainPlayerSeatNumber == value) return;
                _mainPlayerSeatNumber = value;
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
