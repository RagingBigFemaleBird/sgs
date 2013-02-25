using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using Sanguosha.UI.Controls;
using Sanguosha.Lobby.Core;
using System.Windows.Input;

namespace Sanguosha.UI.Controls
{
    public class SeatViewModel : ViewModelBase
    {
        public SeatViewModel()
        {
            ExitCommand = new SimpleRelayCommand((o) => { LobbyViewModel.Instance.ExitRoom(); }) { CanExecuteStatus = true };
            JoinCommand = new SimpleRelayCommand((o) => { LobbyViewModel.Instance.JoinSeat(this); }) { CanExecuteStatus = true };
            CloseCommand = new SimpleRelayCommand((o) => { LobbyViewModel.Instance.CloseSeat(this); }) { CanExecuteStatus = true };
            OpenCommand = new SimpleRelayCommand((o) => { LobbyViewModel.Instance.OpenSeat(this); }) { CanExecuteStatus = true };
            KickCommand = new SimpleRelayCommand((o) => { LobbyViewModel.Instance.KickPlayer(this); }) { CanExecuteStatus = true };
        }

        private Seat _seat;

        public Seat Seat
        {
            get { return _seat; }
            set 
            {
                if (_seat == value) return;
                _seat = value;
                State = value.State;
                Account = value.Account;
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

        private bool _isCurrentSeat;

        public bool IsCurrentSeat
        {
            get { return _isCurrentSeat; }
            set 
            {
                if (_isCurrentSeat == value) return;
                _isCurrentSeat = value;
                OnPropertyChanged("IsCurrentSeat");
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
                if (_state == SeatState.Closed)
                {
                    (JoinCommand as SimpleRelayCommand).CanExecuteStatus = false;
                }
                else
                {
                    (JoinCommand as SimpleRelayCommand).CanExecuteStatus = true;
                }
                OnPropertyChanged("State");
                OnPropertyChanged("IsTaken");
                OnPropertyChanged("IsEmpty");
                OnPropertyChanged("IsClosed");
            }
        }

        public bool IsEmpty
        {
            get
            {
                return _state == SeatState.Empty;
            }
        }

        public bool IsClosed
        {
            get
            {
                return _state == SeatState.Closed;
            }
        }

        public bool IsTaken
        {
            get
            {
                return (_state != SeatState.Empty && _state != SeatState.Closed);
            }
        }

        public double WinRatePercent
        {
            get
            {                
                if (Account == null || Account.TotalGames == 0) return 0;
                return Account.Wins * 100 / Account.TotalGames;                
            }
        }

        public double QuitRatePercent
        {
            get
            {
                if (Account == null || Account.TotalGames == 0) return 0;
                return Account.Quits * 100 / Account.TotalGames;                
            }
        }

        public string WinRate 
        {
            get 
            {
                double result;
                if (Account == null || Account.TotalGames == 0) result = 0; 
                else result = (double)Account.Wins / Account.TotalGames;
                return result.ToString("P1");
            }
        }

        public string QuitRate 
        {
            get
            {
                double result;
                if (Account == null || Account.TotalGames == 0) result = 0;
                else result = (double)Account.Quits / Account.TotalGames;
                return result.ToString("P1");
            }
        }

        public int Level
        {
            get
            {
                int result;
                if (Account == null) result = 0;
                else
                {
                    result = 0;
                    int exp = Account.Experience;
                    while (exp > 10 * result)
                    {
                        exp -= 10 * result;
                        result++;
                    }
                }
                return result;
            }
        }


        #region Commands
        public ICommand ExitCommand
        {
            get;
            set;
        }
        
        public ICommand JoinCommand
        {
            get;
            set;
        }

        public ICommand CloseCommand
        {
            get;
            set;
        }

        public ICommand OpenCommand
        {
            get;
            set;
        }

        public ICommand KickCommand
        {
            get;
            set;
        }
        #endregion
    }
}