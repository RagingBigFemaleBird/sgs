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
            }
        }
    }
}