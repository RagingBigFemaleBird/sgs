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
        static ILobbyService _connection;

        public static ILobbyService Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }
        static LoginToken _loginToken;

        public static LoginToken LoginToken
        {
            get { return _loginToken; }
            set { _loginToken = value; }
        }

        public LobbyViewModel()
        {
            Rooms = new ObservableCollection<RoomViewModel>();
            CreateRoomCommand = new SimpleRelayCommand(o => CreateRoom()) { CanExecuteStatus = true };
            UpdateRoomCommand = new SimpleRelayCommand(o => UpdateRooms()) { CanExecuteStatus = true };
        }

        private RoomViewModel _currentRoom;

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

        public void UpdateRooms()
        {
            var result = _connection.GetRooms(_loginToken, false);
            Rooms.Clear();
            foreach (var room in result)
            {
                Rooms.Add(new RoomViewModel() { Room = room });
            }
        }

        public void CreateRoom()
        {
            var result = _connection.CreateRoom(_loginToken);
            CurrentRoom = new RoomViewModel() { Room = result };
        }

        public ICommand CreateRoomCommand { get; set; }

        public void NotifyRoomUpdate(int id, Room room)
        {
        }

        public void StartGame()
        {
            RoomOperationResult result;
            _connection.RoomOperations(RoomOperation.StartGame, 0, 0, out result);
            if (result != RoomOperationResult.Success) { }
        }


        public void NotifyKicked()
        {
            throw new NotImplementedException();
        }

        public void NotifyGameStart(string ipAddress, int port)
        {

        }

        public SimpleRelayCommand UpdateRoomCommand { get; set; }
    }
}
