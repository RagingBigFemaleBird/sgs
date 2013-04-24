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
            ClientAccount disconnected = null;
            lock (loggedInAccounts)
            {
                if (loggedInAccounts.ContainsKey(username)) disconnected = loggedInAccounts[username];
            }
            Account authenticatedAccount = Authenticate(username, hash);
            if (authenticatedAccount == null)
            {
                retAccount = null;
                return LoginStatus.InvalidUsernameAndPassword;
            }
            var connection = OperationContext.Current.GetCallbackChannel<IGameClient>();
            if (disconnected != null)
            {
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
                    var index = -1;
                    if (room.GameInfo != null) index = room.GameInfo.Accounts.IndexOf(disconnected.Account);
                    if (index >= 0 && room.Room.State == RoomState.Gaming
                        && !room.GameInfo.IsDead[index])
                    {
                        reconnectionString = room.Room.IpAddress.ToString() + ":" + room.Room.IpPort;
                        reconnectionToken = room.GameInfo.LoginTokens[index];
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
                lock (loggedInAccounts)
                {
                    loggedInAccounts.Add(username, acc);
                }
                currentAccount = acc;
                // hack
                var roomresult = from r in rooms.Values where r.Room.Seats.Any(st => st.Account == authenticatedAccount) select r;
                if (roomresult.Count() > 0)
                {
                    acc.CurrentRoom = roomresult.First();
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
                for (int i = 0; i < 8; i++)
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
            if (rooms.ContainsKey(roomId))
            {
                lock (rooms[roomId].Room)
                {
                    if (rooms[roomId].Room.State == RoomState.Gaming) return RoomOperationResult.Locked;
                    int seatNo = 0;
                    foreach (var seat in rooms[roomId].Room.Seats)
                    {
                        Trace.TraceInformation("Testing seat {0}", seatNo);
                        if (seat.Account == null && seat.State == SeatState.Empty)
                        {
                            currentAccount.CurrentRoom = rooms[roomId];
                            seat.Account = currentAccount.Account;
                            seat.State = SeatState.GuestTaken;
                            _NotifyRoomLayoutChanged(roomId);
                            Trace.TraceInformation("Seat {0}", seatNo);
                            room = rooms[roomId].Room;
                            _Unspectate(currentAccount);
                            return RoomOperationResult.Success;
                        }
                        seatNo++;
                    }
                    Trace.TraceInformation("Full");
                }
                return RoomOperationResult.Full;
            }
            return RoomOperationResult.Invalid;
        }

        private static void _DestroyRoom(int roomId)
        {
            lock (rooms)
            {
                if (!rooms.ContainsKey(roomId)) return;
                foreach (var sp in rooms[roomId].Spectators)
                {
                    loggedInAccounts[sp].CurrentSpectatingRoom = null;
                }
                rooms[roomId].Spectators.Clear();
                rooms[roomId].GameInfo = null;
                foreach (var st in rooms[roomId].Room.Seats)
                {
                    st.Account = null;
                    st.State = SeatState.Closed;
                }
                rooms.Remove(roomId);
            }
        }

        private static RoomOperationResult _ExitRoom(ClientAccount account, bool forced = false)
        {
            if (account.CurrentRoom != null)
            {
                var room = account.CurrentRoom;
                foreach (var seat in room.Room.Seats)
                {
                    if (seat.Account == account.Account)
                    {
                        int index = -1;
                        if (room.GameInfo != null) index = room.GameInfo.Accounts.IndexOf(account.Account);
                        if (!forced && room.Room.State == RoomState.Gaming
                            && room.GameInfo != null && index >= 0 && !room.GameInfo.IsDead[index])
                        {
                            return RoomOperationResult.Locked;
                        }
                        lock (room.Room)
                        {
                            bool findAnotherHost = false;
                            if (seat.State == SeatState.Host)
                            {
                                findAnotherHost = true;
                            }
                            seat.Account = null;
                            seat.State = SeatState.Empty;
                            account.CurrentRoom = null;
                            if (!room.Room.Seats.Any(state => state.State != SeatState.Empty && state.State != SeatState.Closed))
                            {
                                _DestroyRoom(room.Room.Id);
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
                        }
                        return RoomOperationResult.Success;
                    }
                }
            }
            return RoomOperationResult.Invalid;
        }

        public RoomOperationResult ExitRoom()
        {
            return _ExitRoom(currentAccount);
        }

        private static void _NotifyRoomLayoutChanged(int roomId)
        {
            try
            {
                foreach (var notify in rooms[roomId].Room.Seats)
                {
                    if (notify.Account != null)
                    {
                        try
                        {
                            lock (loggedInAccounts)
                            {
                                var channel = loggedInAccounts[notify.Account.UserName].CallbackChannel;
                                channel.NotifyRoomUpdate(roomId, rooms[roomId].Room);
                            }
                        }
                        catch (Exception)
                        {
                        }
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
                int i = 0;
                foreach (var notify in rooms[roomId].Room.Seats)
                {
                    if (notify.Account != null)
                    {
                        try
                        {
                            lock (loggedInAccounts)
                            {
                                var channel = loggedInAccounts[notify.Account.UserName].CallbackChannel;
                                channel.NotifyGameStart(ip.ToString() + ":" + port, rooms[roomId].GameInfo.LoginTokens[i]);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        i++;
                    }
                }
            }
            catch (Exception)
            {
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
                lock (rooms[roomId])
                {
                    rooms[roomId].Room.State = RoomState.Waiting;
                    foreach (var seat in rooms[roomId].Room.Seats)
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
                            }

                            if (seat.State != SeatState.Host) seat.State = SeatState.GuestTaken;

                            if ((loggedInAccounts.ContainsKey(seat.Account.UserName) && loggedInAccounts[seat.Account.UserName].CurrentRoom != rooms[roomId]) ||
                                !loggedInAccounts.ContainsKey(seat.Account.UserName))
                            {
                                seat.Account = null;
                                seat.State = SeatState.Empty;
                            }
                        }

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
                    NumberOfDefectors = room.Room.Settings.NumberOfDefectors == 2 ? 2 : 1
                };

                // Load pakcages.
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

                var config = new AccountConfiguration();
                room.GameInfo = config;
                foreach (var addconfig in room.Room.Seats)
                {
                    if (addconfig.Account != null)
                    {
                        config.LoginTokens.Add(new LoginToken() { TokenString = Guid.NewGuid() });
                        config.Accounts.Add(addconfig.Account);
                        config.IsDead.Add(false);
                    }
                }
                GameService.StartGameService(HostingIp, gs, config, room.Room.Id, _OnGameEnds, out portNumber);
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
                if (room.Seats[seatNo].State == SeatState.GuestReady || room.Seats[seatNo].State == SeatState.GuestTaken)
                {
                    var kicked = room.Seats[seatNo].Account;
                    lock (loggedInAccounts)
                    {
                        if (_ExitRoom(loggedInAccounts[kicked.UserName], true) == RoomOperationResult.Invalid)
                        {
                            //zombie occured
                            room.Seats[seatNo].State = SeatState.Empty;
                        }
                        else
                        {
                            loggedInAccounts[kicked.UserName].CallbackChannel.NotifyKicked();
                        }
                    }
                    return RoomOperationResult.Success;
                }
                else
                {
                    room.Seats[seatNo].State = SeatState.Empty;
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
                                loggedInAccounts[seat.Account.UserName].CallbackChannel.NotifyChat(currentAccount.Account, message);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                foreach (var sp in room.Spectators)
                {
                    try
                    {
                        lock (loggedInAccounts)
                        {
                            loggedInAccounts[sp].CallbackChannel.NotifyChat(currentAccount.Account, message);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }) { IsBackground = true };
            thread.Start();
            return RoomOperationResult.Success;
        }

        private static void _Unspectate(ClientAccount account)
        {
            if (account == null || account.Account == null) return;

            if (account.CurrentSpectatingRoom != null)
            {
                account.CurrentSpectatingRoom.Spectators.Remove(account.Account.UserName);
                account.CurrentSpectatingRoom = null;
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
            if (!room.Spectators.Contains(currentAccount.Account.UserName))
            {
                room.Spectators.Add(currentAccount.Account.UserName);
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
