using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Core.Cards
{
    
    public class DefensiveHorse : Equipment
    {
        public override object Clone()
        {
            return Activator.CreateInstance(this.GetType(), this.HorseName);            
        }

        protected override void RegisterEquipmentTriggers(Player p)
        {
            p[Player.RangePlus]++;
        }

        protected override void UnregisterEquipmentTriggers(Player p)
        {
            p[Player.RangePlus]--;
        }

        public override CardCategory Category
        {
            get { return CardCategory.DefensiveHorse; }
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            throw new NotImplementedException();
        }

        public string HorseName { get; set; }
        public DefensiveHorse(string name)
        {
            HorseName = name;
        }

        public override string CardType
        {
            get { return HorseName; }
        }
    }
}
