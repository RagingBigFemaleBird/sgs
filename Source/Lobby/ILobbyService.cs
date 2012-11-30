using Sanguosha.Lobby;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Sanguosha.Lobby
{
    public enum LoginStatus
    {
        Success = 0,
        OutdatedVersion = 1,
        InvalidUsernameAndPassword,
    }
    public struct LoginToken
    {
        public Int64 token;
    }

    public enum RoomOperationResult
    {
        Success = -1,
        Full = -2,
        Password = -3,
        Locked = -4,
    }
    [ServiceContract(Namespace = "", CallbackContract = typeof(IGameClient), SessionMode = SessionMode.Required)]
    public interface ILobbyService
    {
        [OperationContract(IsInitiating = true)]
        LoginStatus Login(int version, string username, out LoginToken token);

        [OperationContract(IsInitiating = false)]
        void Logout(LoginToken token);

        [OperationContract]
        IEnumerable<Room> GetRooms(bool notReadyRoomsOnly);

        [OperationContract]
        int OpenRoom(LoginToken token, string password = null);

        [OperationContract]
        int EnterRoom(LoginToken token, int roomId, bool spectate, string password = null);

        [OperationContract]
        bool ExitRoom(LoginToken token, int roomId);
    }

    public interface IGameClient
    {
        [OperationContract(IsOneWay = true)]
        void NotifyRoomUpdate(int id, Room room);
    }
}
