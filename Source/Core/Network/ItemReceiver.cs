using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.Network
{
    public class ItemReceiver
    {
        private NetworkStream stream;
        static IFormatter formatter = new BinaryFormatter();

        public ItemReceiver(NetworkStream s)
        {
            stream = s;
        }

        public object Receive()
        {
            object o = formatter.Deserialize(stream);
            if (!((o is int) || (o is PlayerItem) || (o is CardItem) || (o is CommandItem) || (o is SkillItem)))
            {
                return null;
            }
            if (o is PlayerItem)
            {
                PlayerItem i = (PlayerItem)o;
                o = Game.CurrentGame.Players[i.id];
            }
            return o;
        }
    }
}
