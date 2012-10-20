using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    public class GuoHeChaiQiao : ShunChai
    {
        protected override string ResultDeckName
        {
            get { return "GuoHeChoice"; }
        }

        protected override string ChoicePrompt
        {
            get { return "GuoHe"; }
        }

        protected override DeckPlace ShunChaiDest(Player source, Player dest)
        {
            return new DeckPlace(null, DeckType.Discard);
        }

        protected override bool ShunChaiAdditionalCheck(Player source, Player dest)
        {
            return true;
        }

        protected override void ShunChaiAddtionalCardReveal(Player source, Player dest, Card card)
        {
            Game.CurrentGame.UpdateCard();
            Game.CurrentGame.RevealCardToAll(card);
        }
    }

}
