using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Sanguosha.LobbyClient;
using Sanguosha.Lobby;

namespace LobbyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var channelFactory = new DuplexChannelFactory<ILobbyService>(new GameClientImpl(), "GameServiceEndpoint");
            ILobbyService server = channelFactory.CreateChannel();
            LoginToken token;
            server.Login(1, "DaMuBie", out token);

            // Do some stuff such as reading messages from the user and sending them to the server

            server.Logout(token);
            channelFactory.Close();
        }
    }
}
