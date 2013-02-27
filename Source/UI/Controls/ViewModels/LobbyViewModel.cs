using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Sanguosha.Lobby.Core;
using System.Windows.Input;
using System.ServiceModel;
using System.Windows;
using System.Threading;
using System.Diagnostics;

namespace Sanguosha.UI.Controls
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class LobbyViewModel : ViewModelBase, IGameClient
    {
        private LobbyViewModel()
        {
            _chatCache = new List<KeyValuePair<string, string>>();
            Rooms = new ObservableCollection<RoomViewModel>();
            UpdateRoomCommand = new SimpleRelayCommand(o => UpdateRooms()) { CanExecuteStatus = true };
            EnterRoomCommand = new SimpleRelayCommand(o => EnterRoom()) { CanExecuteStatus = true };
            StartGameCommand = new SimpleRelayCommand(o => StartGame()) { CanExecuteStatus = false };
            SpectateCommand = new SimpleRelayCommand(o => SpectateGame()) { CanExecuteStatus = true };
            ReadyCommand = new SimpleRelayCommand(o => PlayerReady()) { CanExecuteStatus = true };
            CancelReadyCommand = new SimpleRelayCommand(o => PlayerCancelReady()) { CanExecuteStatus = true};
        }

        private void PlayerCancelReady()        
        {
            var result = _connection.CancelReady(LoginToken);
            if (result == RoomOperationResult.Success)
            {
            }
        }

        private void PlayerReady()
        {
            var result = _connection.Ready(LoginToken);
            if (result == RoomOperationResult.Success)
            {                
            }
        }

        #region Fields
        private static LobbyViewModel _instance;

        /// <summary>
        /// Gets the singleton instance of <c>LobbyViewModel</c>.
        /// </summary>
        public static LobbyViewModel Instance
        {
            get
            {
                if (_instance == null) _instance = new LobbyViewModel();
                return _instance;
            }
        }

        ILobbyService _connection;

        /// <summary>
        /// Gets/sets connection to lobby service. 
        /// </summary>
        public ILobbyService Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        LoginToken _loginToken;

        /// <summary>
        /// Gets/sets current user's login token used for authentication purposes.
        /// </summary>
        public LoginToken LoginToken
        {
            get { return _loginToken; }
            set { _loginToken = value; }
        }


        private RoomViewModel _currentRoom;
                

        /// <summary>
        /// Gets/sets the currrent room that the user is viewing, has entered or is gaming in.
        /// </summary>
        public RoomViewModel CurrentRoom
        {
            get
            {
                return _currentRoom;
            }
            set
            {
                if (_currentRoom == value) return;
                _currentRoom = value;
                OnPropertyChanged("CurrentRoom");
                if (value != null)
                {
                    StartGameCommand.CanExecuteStatus = !(_currentRoom.Seats.Any(s => s.Account != null &&
                                                                                 s.State != SeatState.Host &&
                                                                                 s.State != SeatState.GuestReady))
                                                        && _currentRoom.Seats.Count(s => s.Account != null) >= 2;
                    CurrentSeat = CurrentRoom.Seats.FirstOrDefault(s => s.Account != null && s.Account.UserName == CurrentAccount.UserName);
                }
                else
                {
                    CurrentSeat = null;
                }
            }
        }

        private Account _currentAccount;

        /// <summary>
        /// Gets/sets the currrent room that the user is viewing, has entered or is gaming in.
        /// </summary>
        public Account CurrentAccount
        {
            get
            {
                return _currentAccount;
            }
            set
            {
                if (_currentAccount == value) return;
                _currentAccount = value;
                OnPropertyChanged("CurrentAccount");
            }
        }

        private SeatViewModel _currentSeat;

        /// <summary>
        /// Gets/sets the currrent room that the user is viewing, has entered or is gaming in.
        /// </summary>
        public SeatViewModel CurrentSeat
        {
            get
            {
                return _currentSeat;
            }
            set
            {
                if (_currentSeat == value) return;
                if (_currentSeat != null) _currentSeat.IsCurrentSeat = false;
                _currentSeat = value;
                if (value != null) value.IsCurrentSeat = true;
                OnPropertyChanged("CurrentSeat");
            }
        }

        private ObservableCollection<RoomViewModel> _rooms;

        /// <summary>
        /// Gets/sets all available rooms since last synchronization with the server.
        /// </summary>
        public ObservableCollection<RoomViewModel> Rooms
        {
            get
            {
                return _rooms;
            }
            private set
            {
                if (_rooms == value) return;
                _rooms = value;
                OnPropertyChanged("Rooms");
            }
        }

        private string _gameServerConnectionString;

        public string GameServerConnectionString
        {
            get { return _gameServerConnectionString; }
            set { _gameServerConnectionString = value; }
        }

        #region Commands
        public ICommand UpdateRoomCommand { get; set; }
        public ICommand CreateSingleHeroRoomCommand { get; set; }
        public ICommand CreateDualHeroRoomCommand { get; set; }
        public ICommand EnterRoomCommand { get; set; }
        public SimpleRelayCommand StartGameCommand { get; set; }
        public SimpleRelayCommand SpectateCommand { get; set; }
        public SimpleRelayCommand ReadyCommand { get; set; }
        public SimpleRelayCommand CancelReadyCommand { get; set; }        
        #endregion

        #endregion

        #region Events
        private ChatEventHandler chatEventHandler;

        public event ChatEventHandler OnChat
        {
            add 
            {
                chatEventHandler = value;
                if (value != null)
                {
                    foreach (var cache in _chatCache)
                    {
                        value(cache.Key, cache.Value);
                    }
                    _chatCache.Clear();
                }
            }
            remove 
            {
                if (chatEventHandler == value)
                    chatEventHandler = null;
            }
        }

        #endregion

        #region Public Functions
        /// <summary>
        /// Updates all rooms in the lobby.
        /// </summary>
        public void UpdateRooms()
        {
            var result = _connection.GetRooms(_loginToken, false);
            Rooms.Clear();
            bool found = false;
            foreach (var room in result)
            {
                var model = new RoomViewModel() { Room = room };
                Rooms.Add(model);
                if (CurrentRoom != null && room.Id == CurrentRoom.Id)
                {
                    found = true;
                    CurrentRoom = model;
                }
            }
            if (!found)
            {
                CurrentRoom = null;
            }
        }

        /// <summary>
        /// Creates and enters a new room.
        /// </summary>
        public void CreateRoom(RoomSettings settings)
        {
            var room = _connection.CreateRoom(_loginToken, settings);
            if (room != null)
            {
                CurrentRoom = new RoomViewModel() { Room = room };
                UpdateRooms();
                Trace.Assert(CurrentSeat != null, "Successfully created a room, but do not find myself in the room");
            }
        }

        private bool _IsSuccess(RoomOperationResult result)
        {
            return result == RoomOperationResult.Success;
        }

        public bool EnterRoom()
        {
            Room room;
            if (CurrentSeat != null)
            {
                if (!ExitRoom()) return false;
            }
            if (_IsSuccess(Connection.EnterRoom(_loginToken, _currentRoom.Id, false, null, out room)))
            {
                CurrentRoom = new RoomViewModel() { Room = room };                
                Trace.Assert(CurrentSeat != null, "Successfully joined a room, but do not find myself in the room");
                return true;
            }
            return false;
        }

        public bool ExitRoom()
        {
            if (CurrentRoom == null) return false;
            if (_IsSuccess(Connection.ExitRoom(LoginToken)))
            {
                CurrentSeat = null;
                UpdateRooms();
                return true;
            }
            return false;
        }

        public bool StartGame()
        {
            if (_IsSuccess(_connection.StartGame(_loginToken)))
            {
                CurrentRoom.State = RoomState.Gaming;
                return true;
            }
            return false;
        }

        public bool SpectateGame()
        {
            if (_IsSuccess(_connection.Spectate(_loginToken, _currentRoom.Id)))
            {
                return true;
            }
            return false;
        }

        #region Server Callbacks
        public void NotifyKicked()
        {
            LobbyView.Instance.NotifyKeyEvent(Application.Current.TryFindResource("Lobby.Event.SelfKicked") as string);
            CurrentRoom = null;
            Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate()
            {
                UpdateRooms();
            });
        }

        public void NotifyGameStart(string connectionString)
        {
            GameServerConnectionString = connectionString;
            Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate()
            {
                LobbyView.Instance.StartGame();
            });
        }

        public void NotifyRoomUpdate(int id, Room room)
        {
            Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate()
            {
                var result = Rooms.FirstOrDefault(r => r.Id == id);
                if (result != null)
                {
                    result.Room = room;
                }
                else
                {
                    Rooms.Add(new RoomViewModel() { Room = room });
                }
                if (CurrentRoom != null && CurrentRoom.Id == id)
                {
                    CurrentRoom = new RoomViewModel() { Room = room };                    
                }
            });
        }
        #endregion
        #endregion

        private List<KeyValuePair<string, string>> _chatCache;

        public void NotifyChat(Account act, string message)
        {
            Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate()
            {
                if (_chatCache.Count > 100) _chatCache.RemoveRange(0, 50);
                _chatCache.Add(new KeyValuePair<string, string>(act.UserName, message));                
                var handler = chatEventHandler;
                if (handler != null)
                {
                    foreach (var cache in _chatCache)
                    {
                        handler(cache.Key, cache.Value);
                    }
                    _chatCache.Clear();
                }
            });
        }

        public bool JoinSeat(SeatViewModel seat)
        {
            if (CurrentSeat == null)
            {
                if (!EnterRoom()) return false;
            }
            return _IsSuccess(Connection.ChangeSeat(LoginToken, CurrentRoom.Seats.IndexOf(seat)));
        }

        public bool CloseSeat(SeatViewModel seat)
        {
            return _IsSuccess(Connection.CloseSeat(LoginToken, CurrentRoom.Seats.IndexOf(seat)));
        }

        public bool OpenSeat(SeatViewModel seat)
        {
            return _IsSuccess(Connection.OpenSeat(LoginToken, CurrentRoom.Seats.IndexOf(seat)));
        }

        public bool KickPlayer(SeatViewModel seat)
        {
            return _IsSuccess(Connection.Kick(LoginToken, CurrentRoom.Seats.IndexOf(seat)));
        }

        public bool SendMessage(string msg)
        {
            return _IsSuccess(Connection.Chat(LoginToken, msg));
        }

        public bool Ping()
        {
            return true;
        }

        public void Logout()
        {
            Connection.Logout(LoginToken);
            LoginToken = new LoginToken();
        }
    }

    public delegate void ChatEventHandler(string userName, string msg);
}
