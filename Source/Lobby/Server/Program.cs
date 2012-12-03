using Sanguosha.LobbyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameService = new LobbyServiceImpl();
            var host = new ServiceHost(gameService);

            host.Open();

            Console.WriteLine("Server is running");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            host.Close();
        }
    }
}
