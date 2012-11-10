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

namespace Sanguosha.Expansions.Basic.Cards
{
    [Serializable]
    public class OffensiveHorse : Equipment
    {
        protected override void RegisterEquipmentTriggers(Player p)
        {
            p[Player.RangeMinus]--;
        }

        protected override void UnregisterEquipmentTriggers(Player p)
        {
            p[Player.RangeMinus]++;
        }

        public override CardCategory Category
        {
            get { return CardCategory.OffensiveHorse; }
        }

        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        public string HorseName { get; set; }
        public OffensiveHorse(string name)
        {
            HorseName = name;
        }
        public override string CardType
        {
            get { return HorseName; }
        }
    }
}
