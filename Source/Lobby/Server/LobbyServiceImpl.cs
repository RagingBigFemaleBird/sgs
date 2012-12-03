using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Sanguosha.Lobby;

namespace Sanguosha.LobbyServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    class LobbyServiceImpl : ILobbyService
    {
        Dictionary<int, Room> rooms;
        int roomId;
        Dictionary<Account, Guid> loggedInTokens;
        Dictionary<Guid, Account> loggedInAccounts;
        Dictionary<Guid, IGameClient> loggedInChannels;
        public LobbyServiceImpl()
        {
            rooms = new Dictionary<int, Room>();
            loggedInAccounts = new Dictionary<Guid, Account>();
            loggedInChannels = new Dictionary<Guid, IGameClient>();
            loggedInTokens = new Dictionary<Account, Guid>();
            roomId = 1;
        }

        public LoginStatus Login(int version, string username, out LoginToken token)
        {
            Console.WriteLine("{0} logged in", username);
            var connection = OperationContext.Current.GetCallbackChannel<IGameClient>(); 
            token = new LoginToken();
            token.token = System.Guid.NewGuid();
            Account account = new Account() { Username = username };
            loggedInAccounts.Add(token.token, account);
            loggedInChannels.Add(token.token, connection);
            loggedInTokens.Add(account, token.token);
            return LoginStatus.Success;
        }

        public void Logout(LoginToken token)
        {
            if (loggedInAccounts.ContainsKey(token.token))
            {
                Console.WriteLine("{0} logged out", loggedInAccounts[token.token].Username);
                loggedInAccounts.Remove(token.token);
            }
        }

        public IEnumerable<Room> GetRooms(LoginToken token, bool notReadyRoomsOnly)
        {
            if (loggedInAccounts.ContainsKey(token.token))
            {
                List<Room> ret = new List<Room>();
                foreach (var pair in rooms)
                {
                    if (!notReadyRoomsOnly || !pair.Value.InProgress)
                    {
                        ret.Add(pair.Value);
                    }
                }
                return ret;
            }
            return null;
        }

        public Room CreateRoom(LoginToken token, string password = null)
        {
            if (loggedInAccounts.ContainsKey(token.token))
            {
                while (rooms.ContainsKey(roomId))
                {
                    roomId++;
                }
                Room room = new Room();
                for (int i = 0; i < 8; i++)
                {
                    room.Seats.Add(new Seat() { Ready = false, Disabled = false });
                }
                room.Seats[0].Account = loggedInAccounts[token.token];
                room.Id = roomId;
                rooms.Add(roomId, room);
                Console.WriteLine("created room {0}", roomId);
                return room;
            }
            Console.WriteLine("Invalid createroom call");
            return null;
        }

        public int EnterRoom(LoginToken token, int roomId, bool spectate, string password = null)
        {
            if (loggedInAccounts.ContainsKey(token.token))
            {
                if (rooms.ContainsKey(roomId))
                {
                        if (rooms[roomId].InProgress) return -1;
                        int seatNo = 1;
                        foreach (var seat in rooms[roomId].Seats)
                        {
                            if (seat.Account == null)
                            {
                                seat.Account = loggedInAccounts[token.token];
                                foreach (var notify in rooms[roomId].Seats)
                                {
                                    loggedInChannels[loggedInTokens[notify.Account]].NotifyRoomUpdate(rooms[roomId].Id, rooms[roomId]);
                                }
                                return seatNo;
                            }
                            seatNo++;
                        }
                        return (int)RoomOperationResult.Full;
                }
            }
            return (int)RoomOperationResult.Auth;
        }

        public bool ExitRoom(LoginToken token, int roomId)
        {
            throw new NotImplementedException();
        }

        public bool RoomOperations(RoomOperation op, int arg1, int arg2, out int result)
        {
            throw new NotImplementedException();
        }
    }
}
