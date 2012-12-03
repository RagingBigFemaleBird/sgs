using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanguosha.Lobby;

namespace Sanguosha.LobbyClient
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
}
