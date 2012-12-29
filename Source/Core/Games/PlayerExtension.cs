using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;


namespace Sanguosha.Core.Games
{
    public static class PlayerExtension
    {
        public static List<Card> HandCards(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Hand];
        }

        public static List<Card> Equipments(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Equipment];
        }

        public static List<Card> DelayedTools(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.DelayedTools];
        }

        public static Card Weapon(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Equipment].FirstOrDefault(c => c.Type is Weapon);
        }

        public static Card Armor(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Equipment].FirstOrDefault(c => c.Type is Armor);
        }

        public static Card DefensiveHorse(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Equipment].FirstOrDefault(c => c.Type is DefensiveHorse);
        }

        public static Card OffensiveHorse(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Equipment].FirstOrDefault(c => c.Type is OffensiveHorse);
        }
    }
}
