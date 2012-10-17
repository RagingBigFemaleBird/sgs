using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.Network
{
    public class Server
    {
        private int maxClients;

        /// <summary>
        /// Initialize and start the server.
        /// </summary>
        public Server(int capacity)
        {
            maxClients = capacity;
        }

        /// <summary>
        /// Ready the server. Block if require more clients to connect.
        /// </summary>
        public void Ready()
        {
        }

    }
}
