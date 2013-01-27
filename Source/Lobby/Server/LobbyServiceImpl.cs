using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Sanguosha.Lobby.Core;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace Sanguosha.Lobby.Server
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public class LobbyServiceImpl : ILobbyService
    {
        List<Account> accounts;
        Dictionary<int, Room> rooms;
        int newRoomId;
        int newAccountId;
        Dictionary<Account, Guid> loggedInAccountToGuid;
        Dictionary<Guid, Account> loggedInGuidToAccount;
        Dictionary<IGameClient, Guid> loggedInChannelsToGuid;
        Dictionary<Guid, IGameClient> loggedInGuidToChannel;
        Dictionary<Guid, Room> loggedInGuidToRoom;
        AccountContext accountContext;

        public IPAddress HostingIp { get; set; }

        private bool VerifyClient(LoginToken token)
        {
            if (loggedInGuidToAccount.ContainsKey(token.token))
            {
                if (loggedInGuidToChannel.ContainsKey(token.token))
                {
                    if (loggedInGuidToChannel[token.token] == OperationContext.Current.GetCallbackChannel<IGameClient>())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public LobbyServiceImpl(bool noDatabase = true)
        {
            rooms = new Dictionary<int, Room>();
            loggedInGuidToAccount = new Dictionary<Guid, Account>();
            loggedInGuidToChannel = new Dictionary<Guid, IGameClient>();
            loggedInAccountToGuid = new Dictionary<Account, Guid>();
            loggedInChannelsToGuid = new Dictionary<IGameClient, Guid>();
            loggedInGuidToRoom = new Dictionary<Guid, Room>();
            newRoomId = 1;
            newAccountId = 1;
            CheatEnabled = false;
            accounts = new List<Account>();
            if (noDatabase) accountContext = null;
            else accountContext = new AccountContext();
        }
  
        void Channel_Faulted(object sender, EventArgs e)
        {
            try
            {
                var connection = sender as IGameClient;
                var guid = loggedInChannelsToGuid[connection];
                if (loggedInGuidToRoom[guid].State == RoomState.Gaming) return;
                _Logout(new LoginToken() { token = guid });
            }
            catch (Exception)
            {
            }
        }

        public LoginStatus Login(int version, string username, out LoginToken token, out Account retAccount, out string reconnectionString)
        {
            var disconnected = accounts.FirstOrDefault(ac => ac.UserName == username);
            if (disconnected != null)
            {
                token = new LoginToken() { token = loggedInAccountToGuid[disconnected] };
                var ping = loggedInGuidToChannel[token.token];
                try
                {
                    if (ping.Ping())
                    {
                        reconnectionString = null;
                        retAccount = null;
                        return LoginStatus.InvalidUsernameAndPassword;
                    }
                }
                catch (Exception)
                {
                }
            }
            else
            {
                token = new LoginToken();
                token.token = System.Guid.NewGuid();
            }
            reconnectionString = null;
            if (version != Misc.ProtocolVersion)
            {
                retAccount = null;
                return LoginStatus.OutdatedVersion;
            }
            Console.WriteLine("{0} logged in", username);
            var connection = OperationContext.Current.GetCallbackChannel<IGameClient>();
            OperationContext.Current.Channel.Faulted += new EventHandler(Channel_Faulted);
            OperationContext.Current.Channel.Closed += new EventHandler(Channel_Faulted);
            Account account = disconnected;
            if (account == null)
            {
                account = new Account() { UserName = username, Id = newAccountId++ };
                accounts.Add(account);
                if (accountContext != null)
                {
                    try
                    {
                        accountContext.Accounts.Add(account);
                        accountContext.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.ToString());
                    }
                }
            }
            retAccount = account;
            if (disconnected != null)
            {
                loggedInGuidToAccount.Remove(token.token);
                loggedInAccountToGuid.Remove(disconnected);
                if (loggedInGuidToChannel.ContainsKey(token.token))
                {
                    loggedInChannelsToGuid.Remove(loggedInGuidToChannel[token.token]);
                    loggedInGuidToChannel.Remove(token.token);
                }
            }
            loggedInGuidToAccount.Add(token.token, account);
            loggedInGuidToChannel.Add(token.token, connection);
            loggedInAccountToGuid.Add(account, token.token);
            loggedInChannelsToGuid.Add(connection, token.token);
            if (disconnected != null && loggedInGuidToRoom.ContainsKey(token.token) && loggedInGuidToRoom[token.token].State == RoomState.Gaming)
            {
                reconnectionString = loggedInGuidToRoom[token.token].IpAddress.ToString() + ":" + loggedInGuidToRoom[token.token].IpPort;
            }
            return LoginStatus.Success;
        }

        private void _Logout(LoginToken token)
        {
            if (!loggedInGuidToAccount.ContainsKey(token.token)) return;
            Console.WriteLine("{0} logged out", loggedInGuidToAccount[token.token].UserName);
            if (loggedInGuidToRoom.ContainsKey(token.token))
            {
                _ExitRoom(token);
            }
            loggedInGuidToAccount.Remove(token.token);
        }

        public void Logout(LoginToken token)
        {
            if (VerifyClient(token))
            {
                _Logout(token);
            }
        }

        public IEnumerable<Room> GetRooms(LoginToken token, bool notReadyRoomsOnly)
        {
            if (VerifyClient(token))
            {
                List<Room> ret = new List<Room>();
                foreach (var pair in rooms)
                {
                    if (!notReadyRoomsOnly || pair.Value.State == RoomState.Waiting)
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
            if (VerifyClient(token))
            {
                if (loggedInGuidToRoom.ContainsKey(token.token))
                {
                    return null;
                }

                while (rooms.ContainsKey(newRoomId))
                {
                    newRoomId++;
                }
                Room room = new Room();
                for (int i = 0; i < 8; i++)
                {
                    room.Seats.Add(new Seat() { State = SeatState.Empty });
                }
                room.Seats[0].Account = loggedInGuidToAccount[token.token];
                room.Seats[0].State = SeatState.Host;
                room.Id = newRoomId;
                room.OwnerId = 0;
                rooms.Add(newRoomId, room);
                if (loggedInGuidToRoom.ContainsKey(token.token))
                {
                    loggedInGuidToRoom.Remove(token.token);
                }
                loggedInGuidToRoom.Add(token.token, room);
                Console.WriteLine("created room {0}", newRoomId);
                return room;
            }
            Console.WriteLine("Invalid createroom call");
            return null;
        }

        public RoomOperationResult EnterRoom(LoginToken token, int roomId, bool spectate, string password, out Room room)
        {
            room = null;
            if (VerifyClient(token))
            {
                Console.WriteLine("{1} Enter room {0}", roomId, token.token);
                if (loggedInGuidToRoom.ContainsKey(token.token))
                {
                    return RoomOperationResult.Locked;
                }
                if (rooms.ContainsKey(roomId))
                {
                    if (rooms[roomId].State == RoomState.Gaming) return RoomOperationResult.Locked;
                    int seatNo = 0;
                    foreach (var seat in rooms[roomId].Seats)
                    {
                        Console.WriteLine("Testing seat {0}", seatNo);
                        if (seat.Account == null && seat.State == SeatState.Empty)
                        {
                            loggedInGuidToRoom.Add(token.token, rooms[roomId]);
                            seat.Account = loggedInGuidToAccount[token.token];
                            seat.State = SeatState.GuestTaken;
                            NotifyRoomLayoutChanged(roomId);
                            Console.WriteLine("Seat {0}", seatNo);
                            room = rooms[roomId]; 
                            return RoomOperationResult.Success;
                        }
                        seatNo++;
                    }
                    Console.WriteLine("Full");
                    return RoomOperationResult.Full;
                }
            }
            Console.WriteLine("Rogue enter room calls");
            return RoomOperationResult.Auth;
        }

        private RoomOperationResult _ExitRoom(LoginToken token)
        {
            if (loggedInGuidToRoom.ContainsKey(token.token))
            {
                var room = loggedInGuidToRoom[token.token];
                foreach (var seat in room.Seats)
                {
                    if (seat.Account == loggedInGuidToAccount[token.token])
                    {
                        bool findAnotherHost = false;
                        if (seat.State == SeatState.Host)
                        {
                            findAnotherHost = true;
                        }
                        seat.Account = null;
                        seat.State = SeatState.Empty;
                        loggedInGuidToRoom.Remove(token.token);
                        if (!room.Seats.Any(state => state.State != SeatState.Empty && state.State != SeatState.Closed))
                        {
                            rooms.Remove(room.Id);
                            return RoomOperationResult.Success;
                        }
                        if (findAnotherHost)
                        {
                            foreach (var host in room.Seats)
                            {
                                if (host.Account != null)
                                {
                                    host.State = SeatState.Host;
                                    break;
                                }
                            }
                        }
                        NotifyRoomLayoutChanged(room.Id);
                        return RoomOperationResult.Success;
                    }
                }
            }
            return RoomOperationResult.Invalid;
        }

        public RoomOperationResult ExitRoom(LoginToken token)
        {
            if (VerifyClient(token))
            {
                return _ExitRoom(token);
            }
            return RoomOperationResult.Auth;
        }

        private void NotifyRoomLayoutChanged(int roomId)
        {
            try
            {
                foreach (var notify in rooms[roomId].Seats)
                {
                    if (notify.Account != null)
                    {
                        var channel = loggedInGuidToChannel[loggedInAccountToGuid[notify.Account]];
                        channel.NotifyRoomUpdate(rooms[roomId].Id, rooms[roomId]);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void _NotifyGameStart(int roomId, IPAddress ip, int port)
        {
            try
            {
                foreach (var notify in rooms[roomId].Seats)
                {
                    if (notify.Account != null)
                    {
                        var channel = loggedInGuidToChannel[loggedInAccountToGuid[notify.Account]];
                        channel.NotifyGameStart(ip.ToString() + ":" + port);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public RoomOperationResult ChangeSeat(LoginToken token, int newSeat)
        {
            if (VerifyClient(token))
            {
                if (!loggedInGuidToRoom.ContainsKey(token.token)) { return RoomOperationResult.Locked; }
                var room = loggedInGuidToRoom[token.token];
                if (room.State == RoomState.Gaming)
                {
                    return RoomOperationResult.Locked;
                }
                if (newSeat < 0 || newSeat >= room.Seats.Count) return RoomOperationResult.Invalid;
                var seat = room.Seats[newSeat];
                if (seat.Account == null && seat.State == SeatState.Empty)
                {
                    foreach (var remove in room.Seats)
                    {
                        if (remove.Account == loggedInGuidToAccount[token.token])
                        {
                            seat.State = remove.State;
                            seat.Account = remove.Account;
                            remove.Account = null;
                            remove.State = SeatState.Empty;
                            NotifyRoomLayoutChanged(newRoomId);

                            return RoomOperationResult.Success;
                        }
                    }
                }
                Console.WriteLine("Full");
                return RoomOperationResult.Full;
            }
            return RoomOperationResult.Auth;
        }

        private void _OnGameEnds(int roomId)
        {
            if (rooms.ContainsKey(roomId))
            {
                rooms[roomId].State = RoomState.Waiting;
                foreach (var seat in rooms[roomId].Seats)
                {
                    try
                    {
                        if (seat.Account != null && loggedInGuidToChannel[loggedInAccountToGuid[seat.Account]].Ping())
                        {
                            return;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                foreach (var seat in rooms[roomId].Seats)
                {
                    if (seat.Account != null) _Logout(new LoginToken() { token = loggedInAccountToGuid[seat.Account] });
                }
            }
        }

        public RoomOperationResult StartGame(LoginToken token)
        {
            if (VerifyClient(token))
            {
                if (!loggedInGuidToRoom.ContainsKey(token.token)) { return RoomOperationResult.Invalid; }
                int portNumber;
                var room = loggedInGuidToRoom[token.token];
                var total = room.Seats.Count(pl => pl.Account != null);
                var initiator = room.Seats.FirstOrDefault(pl => pl.Account == loggedInGuidToAccount[token.token]);
                if (room.State == RoomState.Gaming) return RoomOperationResult.Invalid;
                if (total <= 1) return RoomOperationResult.Invalid;
                if (initiator == null || initiator.State != SeatState.Host) return RoomOperationResult.Invalid;
                if (room.Seats.Any(cs => cs.Account != null && cs.State != SeatState.Host && cs.State != SeatState.GuestReady)) return RoomOperationResult.Invalid;
                room.State = RoomState.Gaming;
                foreach (var unready in room.Seats)
                {
                    if (unready.State == SeatState.GuestReady) unready.State = SeatState.GuestTaken;
                }
                var gs = new GameSettings() { TimeOutSeconds = room.TimeOutSeconds, TotalPlayers = total, CheatEnabled = CheatEnabled };
                var config = new AccountConfiguration();
                config.AccountIds = new List<LoginToken>();
                config.Accounts = new List<Account>();
                foreach (var addconfig in room.Seats)
                {
                    if (addconfig.Account != null)
                    {
                        config.AccountIds.Add(new LoginToken() { token = loggedInAccountToGuid[addconfig.Account] });
                        config.Accounts.Add(addconfig.Account);
                    }
                }
                GameService.StartGameService(HostingIp, gs, config, room.Id, _OnGameEnds, out portNumber);
                room.IpAddress = HostingIp.ToString();
                room.IpPort = portNumber;
                _NotifyGameStart(loggedInGuidToRoom[token.token].Id, HostingIp, portNumber);
                return RoomOperationResult.Success;
            }
            return RoomOperationResult.Auth;
        }

        public bool CheatEnabled { get; set; }


        public RoomOperationResult Ready(LoginToken token)
        {
            if (VerifyClient(token))
            {
                if (!loggedInGuidToRoom.ContainsKey(token.token)) { return RoomOperationResult.Invalid; }
                var room = loggedInGuidToRoom[token.token];
                var seat = room.Seats.FirstOrDefault(s => s.Account == loggedInGuidToAccount[token.token]);
                if (seat == null) return RoomOperationResult.Invalid;
                if (seat.State != SeatState.GuestTaken) return RoomOperationResult.Invalid;
                seat.State = SeatState.GuestReady;
                NotifyRoomLayoutChanged(room.Id);
                return RoomOperationResult.Success;
            }
            return RoomOperationResult.Auth;
        }

        public RoomOperationResult CancelReady(LoginToken token)
        {
            if (VerifyClient(token))
            {
                if (!loggedInGuidToRoom.ContainsKey(token.token)) { return RoomOperationResult.Invalid; }
                var room = loggedInGuidToRoom[token.token];
                var seat = room.Seats.FirstOrDefault(s => s.Account == loggedInGuidToAccount[token.token]);
                if (seat == null) return RoomOperationResult.Invalid;
                if (seat.State != SeatState.GuestReady) return RoomOperationResult.Invalid;
                seat.State = SeatState.GuestTaken;
                NotifyRoomLayoutChanged(room.Id);
                return RoomOperationResult.Success;
            }
            return RoomOperationResult.Auth;
        }

        public RoomOperationResult Kick(LoginToken token, int seatNo)
        {
            if (VerifyClient(token))
            {
                if (!loggedInGuidToRoom.ContainsKey(token.token)) { return RoomOperationResult.Invalid; }
                var room = loggedInGuidToRoom[token.token];
                var seat = room.Seats.FirstOrDefault(s => s.Account == loggedInGuidToAccount[token.token]);
                if (seat == null) return RoomOperationResult.Invalid;
                if (seat.State != SeatState.Host) return RoomOperationResult.Invalid;
                if (room.Seats[seatNo].State == SeatState.GuestReady || room.Seats[seatNo].State == SeatState.GuestTaken)
                {
                    var kicked = new LoginToken() { token = loggedInAccountToGuid[room.Seats[seatNo].Account] };
                    _ExitRoom(kicked);
                    loggedInGuidToChannel[kicked.token].NotifyKicked();
                    return RoomOperationResult.Success;
                }
                return RoomOperationResult.Invalid;
            }
            return RoomOperationResult.Auth;
        }

        public RoomOperationResult OpenSeat(LoginToken token, int seatNo)
        {
            if (VerifyClient(token))
            {
                if (!loggedInGuidToRoom.ContainsKey(token.token)) { return RoomOperationResult.Invalid; }
                var room = loggedInGuidToRoom[token.token];
                var seat = room.Seats.FirstOrDefault(s => s.Account == loggedInGuidToAccount[token.token]);
                if (seat == null) return RoomOperationResult.Invalid;
                if (seat.State != SeatState.Host) return RoomOperationResult.Invalid;
                if (room.Seats[seatNo].State != SeatState.Closed) return RoomOperationResult.Invalid;
                room.Seats[seatNo].State = SeatState.Empty;
                NotifyRoomLayoutChanged(room.Id);
                return RoomOperationResult.Success;
            }
            return RoomOperationResult.Auth;
        }

        public RoomOperationResult CloseSeat(LoginToken token, int seatNo)
        {
            if (VerifyClient(token))
            {
                if (!loggedInGuidToRoom.ContainsKey(token.token)) { return RoomOperationResult.Invalid; }
                var room = loggedInGuidToRoom[token.token];
                var seat = room.Seats.FirstOrDefault(s => s.Account == loggedInGuidToAccount[token.token]);
                if (seat == null) return RoomOperationResult.Invalid;
                if (seat.State != SeatState.Host) return RoomOperationResult.Invalid;
                if (room.Seats[seatNo].State != SeatState.Empty) return RoomOperationResult.Invalid;
                room.Seats[seatNo].State = SeatState.Closed;
                NotifyRoomLayoutChanged(room.Id);
            }
            return RoomOperationResult.Auth;
        }


        public RoomOperationResult Chat(LoginToken token, string message)
        {
            if (VerifyClient(token))
            {
                if (message.Length > Misc.MaxChatLength) return RoomOperationResult.Invalid;
                try
                {
                    if (loggedInGuidToRoom.ContainsKey(token.token)/* && loggedInGuidToRoom[token.token].State == RoomState.Gaming*/)
                    {
                        foreach (var seat in loggedInGuidToRoom[token.token].Seats)
                        {
                            if (seat.Account != null)
                            {
                                try
                                {
                                    loggedInGuidToChannel[loggedInAccountToGuid[seat.Account]].NotifyChat(loggedInGuidToAccount[token.token], message);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                    /*else
                    {
                        foreach (var pair in loggedInGuidToChannel)
                        {
                            pair.Value.NotifyChat(loggedInGuidToAccount[token.token], message);
                        }
                    }*/
                }
                catch (Exception)
                {
                }
                return RoomOperationResult.Success;
            }
            return RoomOperationResult.Auth;
        }


        public RoomOperationResult Spectate(LoginToken token, int roomId)
        {
            if (VerifyClient(token))
            {
                if (loggedInGuidToRoom.ContainsKey(token.token)) { return RoomOperationResult.Invalid; }
                var room = rooms[roomId];
                if (room.State != RoomState.Gaming) return RoomOperationResult.Invalid;
                var channel = loggedInGuidToChannel[token.token];
                channel.NotifyGameStart(room.IpAddress + ":" + room.IpPort);
                return RoomOperationResult.Success;
            }
            return RoomOperationResult.Auth;
        }
    }
}
