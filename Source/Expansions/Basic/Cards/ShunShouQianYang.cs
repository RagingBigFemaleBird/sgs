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
    
    public class ShunShouQianYang : ShunChai
    {
        protected override string ResultDeckName
        {
            get { return "ShunShouChoice"; }
        }

        protected override string ChoicePrompt
        {
            get { return "ShunShou"; }
        }

        protected override DeckPlace ShunChaiDest(Player source, Player dest)
        {
            return new DeckPlace(source, DeckType.Hand);
        }

        protected override bool ShunChaiAdditionalCheck(Player source, Player dest, ICard card)
        {
            var args = new AdjustmentEventArgs();
            args.Source = source;
            args.Targets = new List<Player>() { dest };
            args.Card = card;
            args.AdjustmentAmount = 0;
            Game.CurrentGame.Emit(GameEvent.CardRangeModifier, args);
            if (Game.CurrentGame.DistanceTo(source, dest) > 1)
            {
                return false;
            }
            return true;
        }
    }

}
