using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanguosha.Lobby
{
    [Serializable]
    public struct ActiveGame
    {
        public int id;
        public string name;
        public string IP;
        public int port;
    }
}
