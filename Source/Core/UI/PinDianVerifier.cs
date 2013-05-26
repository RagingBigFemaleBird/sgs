using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.UI
{
    public class PinDianVerifier : CardsAndTargetsVerifier
    {
        public PinDianVerifier()
        {
            MaxCards = 0;
            MinPlayers = 1;
            MaxPlayers = 1;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return source != player && player.HandCards().Count > 0;
        }
    }
}
