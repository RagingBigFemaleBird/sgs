using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Sanguosha.Lobby.Core;
using System.Windows.Input;

namespace Sanguosha.UI.Controls
{    
    public class LobbyViewModel : ViewModelBase, IGameClient
    {
        private LobbyViewModel()
        {
            Rooms = new ObservableCollection<RoomViewModel>();
            CreateRoomCommand = new SimpleRelayCommand(o => CreateRoom()) { CanExecuteStatus = true };
            UpdateRoomCommand = new SimpleRelayCommand(o => UpdateRooms()) { CanExecuteStatus = true };
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
        public SimpleRelayCommand UpdateRoomCommand { get; set; }

        public ICommand CreateRoomCommand { get; set; }
        #endregion
        #endregion

        #region Events

        public event EventHandler OnGameInitiated;

        #endregion

        #region Public Functions
        /// <summary>
        /// Updates all rooms in the lobby.
        /// </summary>
        public void UpdateRooms()
        {
            var result = _connection.GetRooms(_loginToken, false);
            Rooms.Clear();
            foreach (var room in result)
            {
                Rooms.Add(new RoomViewModel() { Room = room });
            }
        }

        /// <summary>
        /// Creates and enters a new room.
        /// </summary>
        public void CreateRoom()
        {
            var result = _connection.CreateRoom(_loginToken);
            CurrentRoom = new RoomViewModel() { Room = result };
        }                

        public void StartGame()
        {
            RoomOperationResult result;
            _connection.RoomOperations(RoomOperation.StartGame, 0, 0, out result);
            if (result != RoomOperationResult.Success) { }
        }

        #region Server Callbacks
        public void NotifyKicked()
        {
            throw new NotImplementedException();
        }

        public void NotifyGameStart(string connectionString)
        {
            GameServerConnectionString = connectionString;
            OnGameInitiated(this, new EventArgs());
        }

        public void NotifyRoomUpdate(int id, Room room)
        {
        }
        #endregion
        #endregion


    }
}
