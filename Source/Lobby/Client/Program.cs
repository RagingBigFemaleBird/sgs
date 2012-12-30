using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Sanguosha.Lobby.Client;
using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Client
{
    public class GameClientImpl : IGameClient
    {
        public void NotifyRoomUpdate(int id, Room room)
        {
            Console.WriteLine("Room {0} update", id);
        }

        public void NotifyKicked()
        {
            throw new NotImplementedException();
        }

        public void NotifyGameStart()
        {
            throw new NotImplementedException();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var channelFactory = new DuplexChannelFactory<ILobbyService>(new GameClientImpl(), "GameServiceEndpoint");
            ILobbyService server = channelFactory.CreateChannel();
            LoginToken token;
            server.Login(1, "DaMuBie", out token);

            // Do some stuff such as reading messages from the user and sending them to the server
            var room = server.CreateRoom(token);
            server.EnterRoom(token, 1, false);
            var myRooms = server.GetRooms(token, false);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            server.Logout(token);
            channelFactory.Close();
        }
    }
}
