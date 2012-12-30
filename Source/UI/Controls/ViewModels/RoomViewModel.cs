using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Lobby.Core;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Sanguosha.UI.Controls
{
    public class RoomViewModel : ViewModelBase
    {
        public RoomViewModel()
        {
            Seats = new ObservableCollection<SeatViewModel>();
            LeftSeats = new ObservableCollection<SeatViewModel>();
            RightSeats = new ObservableCollection<SeatViewModel>();
        }

        private int _id;

        public int Id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        public string ModeString
        {
            get { return "RoleGame"; }
        }

        private int _timeOutSeconds;

        public int TimeOutSeconds
        {
            get { return _timeOutSeconds; }
            set 
            {
                if (_timeOutSeconds == value) return;
                _timeOutSeconds = value;
                OnPropertyChanged("TimeOutSeconds");
            }
        }

        public int OpenSeatCount
        {
            get
            {
                return Seats.Count(p => p.State == SeatState.Empty);
            }
        }

        public string OpenSeatString
        {
            get 
            {
                return string.Format("{0}/{1}", OpenSeatCount, Seats.Count); 
            }
        }

        private RoomState _state;

        public RoomState State
        {
            get { return _state; }
            set 
            {
                if (_state == value) return;
                _state = value;
                OnPropertyChanged("State");
            }
        }

        public void ClearSeats()
        {
            LeftSeats.Clear();
            RightSeats.Clear();
            Seats.Clear();
        }

        public void AddSeat(SeatViewModel seat, bool? addToLeft = null)
        {
            if (addToLeft == true)
            {
                _leftSeats.Add(seat);
            }
            else if (addToLeft == false)
            {
                _rightSeats.Add(seat);
            }
            else
            {
                var side = _leftSeats.Count > _rightSeats.Count ? _rightSeats : _leftSeats;
                side.Add(seat);
            }
            _seats.Add(seat);
            Trace.Assert(LeftSeats.Count + RightSeats.Count == Seats.Count);
        }

        public void RemoveSeat(SeatViewModel seat)
        {
            LeftSeats.Remove(seat);
            RightSeats.Remove(seat);
            Seats.Remove(seat);
            Trace.Assert(LeftSeats.Count + RightSeats.Count == Seats.Count);
        }

        private ObservableCollection<SeatViewModel> _leftSeats;
        public ObservableCollection<SeatViewModel> LeftSeats
        {
            get
            {
                return _leftSeats;
            }
            private set
            {
                if (_leftSeats == value) return;
                _leftSeats = value;
                OnPropertyChanged("LeftSeats");
            }
        }

        private ObservableCollection<SeatViewModel> _rightSeats;
        public ObservableCollection<SeatViewModel> RightSeats
        {
            get
            {
                return _rightSeats;
            }
            private set
            {
                if (_rightSeats == value) return;
                _rightSeats = value;
                OnPropertyChanged("RightSeats");
            }
        }

        private ObservableCollection<SeatViewModel> _seats;
        public ObservableCollection<SeatViewModel> Seats
        {
            get
            {
                return _seats;
            }
            private set
            {
                if (_seats == value) return;
                _seats = value;
                OnPropertyChanged("Seats");
            }
        }
    }
}
