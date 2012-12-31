using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using Sanguosha.UI.Controls;
using Sanguosha.Lobby.Core;

namespace Sanguosha.UI.Controls
{
    public class SeatViewModel : ViewModelBase
    {
        public SeatViewModel()
        {
            
        }

        private Seat _seat;

        public Seat Seat
        {
            get { return _seat; }
            set 
            {
                _seat = value;
                State = value.State;
            }
        }

        private Account _account;

        public Account Account
        {
            get
            {
                return _account;
            }
            set
            {
                if (_account == value) return;
                _account = value;
                OnPropertyChanged("Account");
            }
        }

        private SeatState _state;

        public SeatState State 
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state == value) return;
                _state = value;
                OnPropertyChanged("State");
                OnPropertyChanged("IsTaken");
            }
        }

        public bool IsTaken
        {
            get
            {
                return (_state != SeatState.Empty && _state != SeatState.Closed);
            }
        }
    }
}