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

        private Room _room;

        public Room Room
        {
            get { return _room; }
            set
            {
                if (_room == value) return;
                _room = value;
                if (value == null) return;
                Id = value.Id;
                State = value.State;
                Settings = value.Settings;
                ClearSeats();
                foreach (var seat in value.Seats)
                {
                    AddSeat(new SeatViewModel() { Seat = seat });
                }
            }
        }

        RoomSettings _settings;

        public RoomSettings Settings 
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
                OnPropertyChanged("Settings");
            }
        }

        public string ModeString
        {
            get
            {
                if (Settings.IsDualHeroMode && Settings.NumberOfDefectors == 2) return "DualHeroDualDefectorRoleGame";
                else if (Settings.IsDualHeroMode && Settings.NumberOfDefectors == 1) return "DualHeroSingleDefectorRoleGame";
                else if (!Settings.IsDualHeroMode && Settings.NumberOfDefectors == 2) return "SingleHeroDualDefectorRoleGame";
                else if (!Settings.IsDualHeroMode && Settings.NumberOfDefectors == 1) return "SingleHeroSingleDefectorRoleGame";
                else
                {
                    Trace.Assert(false, "Unknown game mode");
                    return "SingleHeroSingleDefectorRoleGame";
                }
            }
        }

        public int EmptySeatCount
        {
            get
            {
                return Seats.Count(p => p.State != SeatState.Empty && p.State != SeatState.Closed);
            }
        }

        public int OpenSeatCount
        {
            get
            {
                return Seats.Count(p => p.State != SeatState.Closed);
            }
        }

        public string OpenSeatString
        {
            get
            {
                return string.Format("{0}/{1}", EmptySeatCount, OpenSeatCount);
            }
        }

        int optionalHeros;

        public int OptionalHeros
        {
            set { optionalHeros = value; }
            get { return optionalHeros; }
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

        public void ChangeSeat(int seatId)
        {
            var result = LobbyViewModel.Instance.Connection.ChangeSeat(LobbyViewModel.Instance.LoginToken, seatId);
            if (result == RoomOperationResult.Locked) { } //cannot change seat locked
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
