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
using System.IO;
using Sanguosha.Core.Utils;

namespace Sanguosha.Lobby.Server
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.PerSession)]
    public class LobbyServiceImpl : ILobbyService
    {
        static Dictionary<string, ClientAccount> loggedInAccounts = new Dictionary<string, ClientAccount>();
        static Dictionary<int, ServerRoom> rooms = new Dictionary<int, ServerRoom>();
        static int newRoomId = 1;

        public static IPAddress HostingIp { get; set; }
        public static bool CheatEnabled { get; set; }

        static AccountContext accountContext = null;

        private ClientAccount currentAccount;

        public static bool EnableDatabase()
        {
            if (accountContext == null)
            {
                accountContext = new AccountContext();
                accountContext.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");
            }
            else return false;
            return true;
        }

        public LobbyServiceImpl()
        {
            currentAccount = null;
        }

        private Account Authenticate(string username, string hash)
        {
            if (accountContext == null) return new Account() { UserName = username };
            lock (accountContext)
            {
                var result = from a in accountContext.Accounts where a.UserName.Equals(username) select a;
                if (result.Count() == 0) return null;
                if (!result.First().Password.Equals(hash)) return null;
                return result.First();
            }
        }

        public LoginStatus Login(int version, string username, string hash, out Account retAccount, out string reconnectionString, out LoginToken reconnectionToken)
        {
            reconnectionToken = new LoginToken();
            reconnectionString = null;
            if (version != Misc.ProtocolVersion)
            {
                retAccount = null;
                return LoginStatus.OutdatedVersion;
            }

            Account authenticatedAccount = Authenticate(username, hash);
            if (authenticatedAccount == null)
            {
                retAccount = null;
                return LoginStatus.InvalidUsernameAndPassword;
            }
            var connection = OperationContext.Current.GetCallbackChannel<IGameClient>();
            lock (loggedInAccounts)
            {
                ClientAccount disconnected = null;
                if (loggedInAccounts.ContainsKey(username))
                {
                    disconnected = loggedInAccounts[username];
                    var ping = disconnected.CallbackChannel;
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
                    disconnected.CallbackChannel = connection;
                    currentAccount = disconnected;
                    var room = disconnected.CurrentRoom;
                    if (room != null)
                    {
                        if (room.Room.State == RoomState.Gaming
                            && !disconnected.Account.IsDead)
                        {
                            reconnectionString = room.Room.IpAddress.ToString() + ":" + room.Room.IpPort;
                            reconnectionToken = disconnected.Account.LoginToken;
                        }
                        else
                        {
                            disconnected.CurrentRoom = null;
                        }
                    }
                }
                else
                {
                    var acc = new ClientAccount()
                    {
                        Account = authenticatedAccount,
                        CallbackChannel = connection,
                        LobbyService = this
                    };
                    loggedInAccounts.Add(username, acc);
                    currentAccount = acc;
                    // hack
                    var roomresult = from r in rooms.Values where r.Room.Seats.Any(st => st.Account == authenticatedAccount) select r;
                    if (roomresult.Count() > 0)
                    {
                        acc.CurrentRoom = roomresult.First();
                    }
                }
            }
            Trace.TraceInformation("{0} logged in", username);
            EventHandler faultHandler = (o, s) =>
                        {
                            try
                            {
                                if (currentAccount.CurrentRoom.Room.State == RoomState.Gaming) return;
                                _Logout(currentAccount);
                            }
                            catch (Exception)
                            {
                            }
                        };

            OperationContext.Current.Channel.Faulted += faultHandler;
            OperationContext.Current.Channel.Closed += faultHandler;
            retAccount = currentAccount.Account;
            _Unspectate(currentAccount);
            return LoginStatus.Success;
        }

        private static void _Logout(ClientAccount account)
        {
            Trace.TraceInformation("{0} logged out", account.Account.UserName);
            if (account == null || account.LobbyService == null ||
                account.LobbyService.currentAccount == null) return;
            if (account.CurrentRoom != null)
            {
                if (_ExitRoom(account) != RoomOperationResult.Success) return;
            }
            lock (loggedInAccounts)
            {
                Trace.Assert(loggedInAccounts.ContainsKey(account.Account.UserName));
                if (!loggedInAccounts.ContainsKey(account.Account.UserName)) return;
                account.LobbyService.currentAccount = null;
                account.CurrentSpectatingRoom = null;
                loggedInAccounts.Remove(account.Account.UserName);
            }
        }

        public void Logout()
        {
            if (currentAccount == null) return;
            Trace.TraceInformation("{0} logged out", currentAccount.Account.UserName);
            _Logout(currentAccount);
            currentAccount = null;
        }

        public IEnumerable<Room> GetRooms(bool notReadyRoomsOnly)
        {
            if (currentAccount == null) return null;
            lock (rooms)
            {
                return from r in rooms.Values
                       where (!notReadyRoomsOnly || r.Room.State == RoomState.Waiting)
                       select r.Room;
            }
        }

        public Room CreateRoom(RoomSettings settings, string password = null)
        {
            if (currentAccount == null) return null;
            if (currentAccount.CurrentRoom != null)
            {
                return null;
            }

            lock (rooms)
            {
                while (rooms.ContainsKey(newRoomId))
                {
                    newRoomId++;
                }
                Room room = new Room();
                int maxSeats = settings.GameType == GameType.Pk1v1 ? 2 : 8;
                for (int i = 0; i < maxSeats; i++)
                {
                    room.Seats.Add(new Seat() { State = SeatState.Empty });
                }
                room.Seats[0].Account = currentAccount.Account;
                room.Seats[0].State = SeatState.Host;
                room.Id = newRoomId;
                room.OwnerId = 0;
                room.Settings = settings;
                var srvRoom = new ServerRoom() { Room = room };
                rooms.Add(newRoomId, srvRoom);
                currentAccount.CurrentRoom = srvRoom;
                Trace.TraceInformation("created room {0}", newRoomId);
                return room;
            }
        }

        public RoomOperationResult EnterRoom(int roomId, bool spectate, string password, out Room room)
        {
            room = null;
            if (currentAccount == null) return RoomOperationResult.NotAutheticated;
            Trace.TraceInformation("{1} Enter room {0}", roomId, currentAccount.Account.UserName);
            if (currentAccount.CurrentRoom != null)
            {
                return RoomOperationResult.Locked;
            }

            ServerRoom serverRoom = null;
            Room clientRoom = null;
            lock (rooms)
            {
                if (!rooms.ContainsKey(roomId))
                {
                    return RoomOperationResult.Invalid;
                }
                else
                {
                    serverRoom = rooms[roomId];
                    clientRoom = serverRoom.Room;
                }
            }

            lock (clientRoom)
            {
                if (clientRoom.IsEmpty || clientRoom.State == RoomState.Gaming) return RoomOperationResult.Locked;
                int seatNo = 0;
                foreach (var seat in clientRoom.Seats)
                {
                    Trace.TraceInformation("Testing seat {0}", seatNo);
                    if (seat.Account == null && seat.State == SeatState.Empty)
                    {
                        currentAccount.CurrentRoom = serverRoom;
                        seat.Account = currentAccount.Account;
                        seat.State = SeatState.GuestTaken;
                        _NotifyRoomLayoutChanged(roomId);
                        Trace.TraceInformation("Seat {0}", seatNo);
                        _Unspectate(currentAccount);
                        room = clientRoom;
                        return RoomOperationResult.Success;
                    }
                    seatNo++;
                }
                Trace.TraceInformation("Full");
            }
            return RoomOperationResult.Full;
        }

        private static void _DestroyRoom(int roomId)
        {
            ServerRoom room;
            lock (rooms)
            {
                if (!rooms.ContainsKey(roomId)) return;
                room = rooms[roomId];
                rooms.Remove(roomId);
            }
            lock (room.Spectators)
            {
                foreach (var sp in room.Spectators)
                {
                    lock (loggedInAccounts)
                    {
                        if (loggedInAccounts.ContainsKey(sp))
                        {
                            loggedInAccounts[sp].CurrentSpectatingRoom = null;
                        }
                    }
                }
                room.Spectators.Clear();
                foreach (var st in room.Room.Seats)
                {
                    st.Account = null;
                    st.State = SeatState.Closed;
                }
            }
        }

        private static RoomOperationResult _ExitRoom(ClientAccount account, bool forced = false)
        {
            if (account == null) return RoomOperationResult.Invalid;
            var room = account.CurrentRoom;
            if (room == null) return RoomOperationResult.Invalid;

            lock (room.Room)
            {
                var seat = room.Room.Seats.FirstOrDefault(s => s.Account == account.Account);
                if (seat == null) return RoomOperationResult.Invalid;

                if (!forced && room.Room.State == RoomState.Gaming
                     && !account.Account.IsDead)
                {
                    return RoomOperationResult.Locked;
                }

                bool findAnotherHost = false;
                if (seat.State == SeatState.Host)
                {
                    findAnotherHost = true;
                }
                seat.Account = null;
                seat.State = SeatState.Empty;
                account.CurrentRoom = null;

                if (_DestroyRoomIfEmpty(room))
                {
                    return RoomOperationResult.Success;
                }

                if (findAnotherHost)
                {
                    foreach (var host in room.Room.Seats)
                    {
                        if (host.Account != null)
                        {
                            host.State = SeatState.Host;
                            break;
                        }
                    }
                }
                _NotifyRoomLayoutChanged(room.Room.Id);
                return RoomOperationResult.Success;
            }
        }

        private static bool _DestroyRoomIfEmpty(ServerRoom room)
        {
            if (!room.Room.IsEmpty)
            {
                return false;
            }
            else
            {
                _DestroyRoom(room.Room.Id);
                return true;
            }
        }

        public RoomOperationResult ExitRoom()
        {
            return _ExitRoom(currentAccount);
        }

        private static void _NotifyRoomLayoutChanged(int roomId)
        {
            var room = rooms[roomId];
            if (room == null || room.Room == null) return;
            foreach (var notify in room.Room.Seats)
            {
                if (notify.Account != null)
                {
                    try
                    {
                        lock (loggedInAccounts)
                        {
                            var channel = loggedInAccounts[notify.Account.UserName].CallbackChannel;
                            channel.NotifyRoomUpdate(roomId, room.Room);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void _NotifyGameStart(int roomId, IPAddress ip, int port)
        {
            var room = rooms[roomId];
            if (room == null || room.Room == null) return;
            lock (room.Room)
            {
                int i = 0;
                foreach (var notify in room.Room.Seats)
                {
                    if (notify.Account != null)
                    {
                        try
                        {
                            lock (loggedInAccounts)
                            {
                                var channel = loggedInAccounts[notify.Account.UserName].CallbackChannel;
                                channel.NotifyGameStart(ip.ToString() + ":" + port, notify.Account.LoginToken);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        i++;
                    }
                }
            }
        }

        public RoomOperationResult ChangeSeat(int newSeat)
        {
            if (currentAccount == null) return RoomOperationResult.NotAutheticated;
            if (currentAccount.CurrentRoom == null) return RoomOperationResult.Invalid;
            var room = currentAccount.CurrentRoom.Room;
            lock (room)
            {
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
                        if (remove.Account == currentAccount.Account)
                        {
                            if (remove == seat) return RoomOperationResult.Invalid;
                            seat.State = remove.State;
                            seat.Account = remove.Account;
                            remove.Account = null;
                            remove.State = SeatState.Empty;
                            _NotifyRoomLayoutChanged(room.Id);

                            return RoomOperationResult.Success;
                        }
                    }
                }
            }
            Trace.TraceInformation("Full");
            return RoomOperationResult.Full;
        }

        private void _OnGameEnds(int roomId)
        {
            if (accountContext != null)
            {
                try
                {
                    lock (accountContext)
                    {
                        accountContext.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    var crashReport = new StreamWriter(FileRotator.CreateFile("./Crash", "crash", ".dmp", 1000));
                    crashReport.WriteLine(e);
                    crashReport.WriteLine(e.Message);
                    crashReport.Close();
                    accountContext = new AccountContext();
                }
            }
            if (rooms.ContainsKey(roomId))
            {
                var room = rooms[roomId];
                lock (room.Room)
                {
                    room.Room.State = RoomState.Waiting;
                    foreach (var seat in room.Room.Seats)
                    {
                        if (seat.Account == null) continue;
                        lock (loggedInAccounts)
                        {
                            if (loggedInAccounts.ContainsKey(seat.Account.UserName))
                            {
                                try
                                {
                                    loggedInAccounts[seat.Account.UserName].CallbackChannel.Ping();
                                }
                                catch (Exception)
                                {
                                    _Logout(loggedInAccounts[seat.Account.UserName]);
                                    seat.Account = null;
                                    seat.State = SeatState.Empty;
                                    continue;
                                }
                            }
                            else
                            {
                                seat.State = SeatState.Empty;
                                seat.Account = null;
                            }

                            if (seat.State != SeatState.Host) seat.State = SeatState.GuestTaken;

                            if (seat.Account != null && (loggedInAccounts.ContainsKey(seat.Account.UserName) && loggedInAccounts[seat.Account.UserName].CurrentRoom != rooms[roomId]))
                            {
                                seat.Account = null;
                                seat.State = SeatState.Empty;
                            }
                        }
                    }

                    if (_DestroyRoomIfEmpty(room))
                    {
                        return;
                    }
                    if (!room.Room.Seats.Any(st => st.State == SeatState.Host))
                    {
                        var f = room.Room.Seats.First(st => st.State == SeatState.GuestTaken);
                        f.State = SeatState.Host;
                    }
                }
            }
            _NotifyRoomLayoutChanged(roomId);
        }

        public RoomOperationResult StartGame()
        {
            if (currentAccount == null) return RoomOperationResult.NotAutheticated;
            if (currentAccount.CurrentRoom == null) { return RoomOperationResult.Invalid; }
            int portNumber;
            var room = currentAccount.CurrentRoom;
            var total = room.Room.Seats.Count(pl => pl.Account != null);
            var initiator = room.Room.Seats.FirstOrDefault(pl => pl.Account == currentAccount.Account);
            if (room.Room.State == RoomState.Gaming) return RoomOperationResult.Invalid;
            if (total <= 1) return RoomOperationResult.Invalid;
            if (initiator == null || initiator.State != SeatState.Host) return RoomOperationResult.Invalid;
            if (room.Room.Seats.Any(cs => cs.Account != null && cs.State != SeatState.Host && cs.State != SeatState.GuestReady)) return RoomOperationResult.Invalid;
            lock (room.Room)
            {
                room.Room.State = RoomState.Gaming;
                foreach (var unready in room.Room.Seats)
                {
                    if (unready.State == SeatState.GuestReady) unready.State = SeatState.Gaming;
                }
                var gs = new GameSettings()
                {
                    TimeOutSeconds = room.Room.Settings.TimeOutSeconds,
                    TotalPlayers = total,
                    CheatEnabled = CheatEnabled,
                    DualHeroMode = room.Room.Settings.IsDualHeroMode,
                    NumHeroPicks = room.Room.Settings.NumHeroPicks,
                    NumberOfDefectors = room.Room.Settings.NumberOfDefectors == 2 ? 2 : 1,
                    GameType = room.Room.Settings.GameType,
                };

                // Load pakcages.
                if (gs.GameType == GameType.RoleGame)
                {
                    gs.PackagesEnabled.Add("Sanguosha.Expansions.BasicExpansion");
                    gs.PackagesEnabled.Add("Sanguosha.Expansions.BattleExpansion");
                    if ((room.Room.Settings.EnabledPackages & EnabledPackages.Wind) != 0) gs.PackagesEnabled.Add("Sanguosha.Expansions.WindExpansion");
                    if ((room.Room.Settings.EnabledPackages & EnabledPackages.Fire) != 0) gs.PackagesEnabled.Add("Sanguosha.Expansions.FireExpansion");
                    if ((room.Room.Settings.EnabledPackages & EnabledPackages.Woods) != 0) gs.PackagesEnabled.Add("Sanguosha.Expansions.WoodsExpansion");
                    if ((room.Room.Settings.EnabledPackages & EnabledPackages.Hills) != 0) gs.PackagesEnabled.Add("Sanguosha.Expansions.HillsExpansion");
                    if ((room.Room.Settings.EnabledPackages & EnabledPackages.SP) != 0)
                    {
                        gs.PackagesEnabled.Add("Sanguosha.Expansions.SpExpansion");
                        gs.PackagesEnabled.Add("Sanguosha.Expansions.StarSpExpansion");
                    }
                    if ((room.Room.Settings.EnabledPackages & EnabledPackages.OverKnightFame) != 0)
                    {
                        gs.PackagesEnabled.Add("Sanguosha.Expansions.OverKnightFame11Expansion");
                        gs.PackagesEnabled.Add("Sanguosha.Expansions.OverKnightFame12Expansion");
                        gs.PackagesEnabled.Add("Sanguosha.Expansions.OverKnightFame13Expansion");
                    }
                    if ((room.Room.Settings.EnabledPackages & EnabledPackages.Others) != 0)
                    {
                        gs.PackagesEnabled.Add("Sanguosha.Expansions.AssasinExpansion");
                    }
                }
                if (gs.GameType == GameType.Pk1v1)
                {
                    gs.PackagesEnabled.Add("Sanguosha.Expansions.Pk1v1Expansion");
                }

                foreach (var addconfig in room.Room.Seats)
                {
                    var account = addconfig.Account;
                    if (account != null)
                    {
                        account.LoginToken = new LoginToken() { TokenString = Guid.NewGuid() };
                        account.IsDead = false;
                        gs.Accounts.Add(account);
                    }
                }
                GameService.StartGameService(HostingIp, gs, room.Room.Id, _OnGameEnds, out portNumber);
                room.Room.IpAddress = HostingIp.ToString();
                room.Room.IpPort = portNumber;
                _NotifyGameStart(room.Room.Id, HostingIp, portNumber);
            }
            return RoomOperationResult.Success;
        }

        public RoomOperationResult Ready()
        {
            if (currentAccount == null) return RoomOperationResult.NotAutheticated;
            if (currentAccount.CurrentRoom == null) { return RoomOperationResult.Invalid; }
            var room = currentAccount.CurrentRoom.Room;
            lock (room)
            {
                var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
                if (seat == null) return RoomOperationResult.Invalid;
                if (seat.State != SeatState.GuestTaken) return RoomOperationResult.Invalid;
                seat.State = SeatState.GuestReady;
                _NotifyRoomLayoutChanged(room.Id);
            }
            return RoomOperationResult.Success;
        }

        public RoomOperationResult CancelReady()
        {
            if (currentAccount == null) return RoomOperationResult.NotAutheticated;
            if (currentAccount.CurrentRoom == null) { return RoomOperationResult.Invalid; }
            var room = currentAccount.CurrentRoom.Room;
            lock (room)
            {
                var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
                if (seat == null) return RoomOperationResult.Invalid;
                if (seat.State != SeatState.GuestReady) return RoomOperationResult.Invalid;
                seat.State = SeatState.GuestTaken;
                _NotifyRoomLayoutChanged(room.Id);
            }
            return RoomOperationResult.Success;
        }

        public RoomOperationResult Kick(int seatNo)
        {
            if (currentAccount == null) return RoomOperationResult.NotAutheticated;
            if (currentAccount.CurrentRoom == null) { return RoomOperationResult.Invalid; }
            var room = currentAccount.CurrentRoom.Room;
            lock (room)
            {
                var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
                if (seat == null) return RoomOperationResult.Invalid;
                if (seat.State != SeatState.Host) return RoomOperationResult.Invalid;
                if (seatNo < 0 || seatNo >= room.Seats.Count) return RoomOperationResult.Invalid;
                if (room.Seats[seatNo].State == SeatState.GuestReady || room.Seats[seatNo].State == SeatState.GuestTaken)
                {
                    var kicked = room.Seats[seatNo].Account;
                    
                    lock (loggedInAccounts)
                    {
                        if (kicked == null || !loggedInAccounts.ContainsKey(kicked.UserName) ||
                            _ExitRoom(loggedInAccounts[kicked.UserName], true) == RoomOperationResult.Invalid)
                        {
                            // zombie occured?
                            room.Seats[seatNo].State = SeatState.Empty;
                            room.Seats[seatNo].Account = null;
                        }
                        else
                        {
                            try
                            {
                                loggedInAccounts[kicked.UserName].CallbackChannel.NotifyKicked();
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    return RoomOperationResult.Success;
                }
                else
                {
                    room.Seats[seatNo].State = SeatState.Empty;
                    room.Seats[seatNo].Account = null;
                }
            }
            return RoomOperationResult.Invalid;
        }

        public RoomOperationResult OpenSeat(int seatNo)
        {
            if (currentAccount == null) return RoomOperationResult.NotAutheticated;
            if (currentAccount.CurrentRoom == null) { return RoomOperationResult.Invalid; }
            var room = currentAccount.CurrentRoom.Room;
            lock (room)
            {
                var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
                if (seat == null) return RoomOperationResult.Invalid;
                if (seat.State != SeatState.Host) return RoomOperationResult.Invalid;
                if (seatNo < 0 || seatNo >= room.Seats.Count) return RoomOperationResult.Invalid;
                if (room.Seats[seatNo].State != SeatState.Closed) return RoomOperationResult.Invalid;
                room.Seats[seatNo].State = SeatState.Empty;
                _NotifyRoomLayoutChanged(room.Id);
            }
            return RoomOperationResult.Success;
        }

        public RoomOperationResult CloseSeat(int seatNo)
        {
            if (currentAccount == null) return RoomOperationResult.NotAutheticated;
            if (currentAccount.CurrentRoom == null) { return RoomOperationResult.Invalid; }
            var room = currentAccount.CurrentRoom.Room;
            lock (room)
            {
                var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
                if (seat == null) return RoomOperationResult.Invalid;
                if (seat.State != SeatState.Host) return RoomOperationResult.Invalid;
                if (seatNo < 0 || seatNo >= room.Seats.Count) return RoomOperationResult.Invalid;
                if (room.Seats[seatNo].State != SeatState.Empty) return RoomOperationResult.Invalid;
                room.Seats[seatNo].State = SeatState.Closed;
                _NotifyRoomLayoutChanged(room.Id);
            }
            return RoomOperationResult.Success;
        }

        public RoomOperationResult Chat(string message)
        {
            if (currentAccount == null) return RoomOperationResult.NotAutheticated;
            if (message.Length > Misc.MaxChatLength) return RoomOperationResult.Invalid;

            // @todo: No global chat
            if (currentAccount.CurrentRoom == null && currentAccount.CurrentSpectatingRoom == null)
            {
                return RoomOperationResult.Invalid;
            }

            Thread thread = new Thread(() =>
            {
                var room = currentAccount.CurrentRoom;
                if (room == null && currentAccount.CurrentSpectatingRoom != null)
                    room = currentAccount.CurrentSpectatingRoom;
                foreach (var seat in room.Room.Seats)
                {
                    if (seat.Account != null)
                    {
                        try
                        {
                            lock (loggedInAccounts)
                            {
                                if (loggedInAccounts.ContainsKey(seat.Account.UserName))
                                {
                                    loggedInAccounts[seat.Account.UserName].CallbackChannel.NotifyChat(currentAccount.Account, message);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                lock (room.Spectators)
                {
                    foreach (var sp in room.Spectators)
                    {
                        try
                        {
                            lock (loggedInAccounts)
                            {
                                if (loggedInAccounts.ContainsKey(sp))
                                {
                                    loggedInAccounts[sp].CallbackChannel.NotifyChat(currentAccount.Account, message);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }) { IsBackground = true };
            thread.Start();
            return RoomOperationResult.Success;
        }

        private static void _Unspectate(ClientAccount account)
        {
            if (account == null || account.Account == null) return;

            var room = account.CurrentSpectatingRoom;
            if (room != null)
            {
                lock (room.Spectators)
                {
                    room.Spectators.Remove(account.Account.UserName);
                    account.CurrentSpectatingRoom = null;
                }
            }
        }

        public RoomOperationResult Spectate(int roomId)
        {
            if (currentAccount == null) return RoomOperationResult.NotAutheticated;
            if (currentAccount.CurrentRoom != null) { return RoomOperationResult.Invalid; }
            if (!rooms.ContainsKey(roomId)) return RoomOperationResult.Invalid;
            var room = rooms[roomId];
            if (room.Room.State != RoomState.Gaming) return RoomOperationResult.Invalid;
            _Unspectate(currentAccount);
            lock (room.Spectators)
            {
                if (!room.Spectators.Contains(currentAccount.Account.UserName))
                {
                    room.Spectators.Add(currentAccount.Account.UserName);
                }
            }
            currentAccount.CurrentSpectatingRoom = room;
            var channel = currentAccount.CallbackChannel;
            channel.NotifyGameStart(room.Room.IpAddress + ":" + room.Room.IpPort, new LoginToken() { TokenString = new Guid() });
            return RoomOperationResult.Success;
        }

        public static void WipeDatabase()
        {
            AccountContext ctx;
            ctx = new AccountContext();
            ctx.Database.Delete();
        }


        public LoginStatus CreateAccount(string userName, string p)
        {
            if (accountContext == null)
            {
                return LoginStatus.Success;
            }

            lock (accountContext)
            {
                var result = from a in accountContext.Accounts where a.UserName.Equals(userName) select a;
                if (result.Count() != 0)
                {
                    return LoginStatus.InvalidUsernameAndPassword;
                }
                accountContext.Accounts.Add(new Account() { UserName = userName, Password = p });
                accountContext.SaveChanges();
                return LoginStatus.Success;
            }
        }

        public void SubmitBugReport(System.IO.Stream s)
        {
            if (s == null) return;
            try
            {
                Stream file = FileRotator.CreateFile("./Reports", "crashdmp", ".rpt", 1000);

                if (s != null)
                {
                    s.CopyTo(file);
                    s.Flush();
                    s.Close();
                }
                file.Flush();
                file.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}
