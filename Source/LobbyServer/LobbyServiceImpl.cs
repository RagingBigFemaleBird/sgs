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
        public LoginStatus Login(int version, string username, out LoginToken token)
        {
            Console.WriteLine("{0} logged in", username);
            token = new LoginToken();
            return LoginStatus.Success;
        }

        public void Logout(LoginToken token)
        {
            Console.WriteLine("logged out");
        }

        public IEnumerable<Room> GetRooms(bool notReadyRoomsOnly)
        {
            throw new NotImplementedException();
        }

        public bool ExitRoom(LoginToken token, int roomId)
        {
            throw new NotImplementedException();
        }


        public int OpenRoom(LoginToken token, string password = null)
        {
            throw new NotImplementedException();
        }

        public int EnterRoom(LoginToken token, int roomId, bool spectate, string password = null)
        {
            throw new NotImplementedException();
        }
    }
}
