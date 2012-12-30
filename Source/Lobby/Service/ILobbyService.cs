using Sanguosha.Lobby;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;

namespace Sanguosha.Lobby.Core
{
    public enum LoginStatus
    {
        Success = 0,
        OutdatedVersion = 1,
        InvalidUsernameAndPassword,
    }
    public struct LoginToken
    {
        public Guid token;
    }

    public enum RoomOperationResult
    {
        Success = 0,
        Auth = -1,
        Full = -2,
        Password = -3,
        Locked = -4,
    }

    public enum RoomOperation
    {
        ChangeSeat,
        StartGame,
        Kick,
        ChangeOptions,
    }

    [ServiceKnownType("GetKnownTypes", typeof(Helper))]
    [ServiceContract(Namespace = "", CallbackContract = typeof(IGameClient), SessionMode = SessionMode.Required)]
    public interface ILobbyService
    {
        [OperationContract(IsInitiating = true)]
        LoginStatus Login(int version, string username, out LoginToken token);

        [OperationContract(IsInitiating = false)]
        void Logout(LoginToken token);

        [OperationContract]
        IEnumerable<Room> GetRooms(LoginToken token, bool notReadyRoomsOnly);

        [OperationContract]
        Room CreateRoom(LoginToken token, string password = null);

        [OperationContract]
        int EnterRoom(LoginToken token, int roomId, bool spectate, string password = null);

        [OperationContract]
        bool ExitRoom(LoginToken token, int roomId);

        [OperationContract]
        bool RoomOperations(RoomOperation op, int arg1, int arg2, out RoomOperationResult result);
    }

    public interface IGameClient
    {
        [OperationContract(IsOneWay = true)]
        void NotifyRoomUpdate(int id, Room room);

        void NotifyKicked();

        void NotifyGameStart(string connectionString);
    }

    static class Helper
    {
        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
        {
            System.Collections.Generic.List<System.Type> knownTypes =
                new System.Collections.Generic.List<System.Type>();
            // Add any types to include here.
            knownTypes.Add(typeof(Room));
            knownTypes.Add(typeof(Seat));
            knownTypes.Add(typeof(Account));
            return knownTypes;
        }
    }

}
