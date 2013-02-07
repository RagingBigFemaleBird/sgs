using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Players;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Heroes;
using System.Threading;
using Sanguosha.Core.Games;
using Sanguosha.Core.Utils;
using System.Diagnostics;

namespace Sanguosha.Core.Skills
{
    public class CleanupSquad : Trigger
    {
        Dictionary<ISkill, List<DeckType>> deckCleanup;
        Dictionary<ISkill, List<PlayerAttribute>> markCleanup;

        public CleanupSquad()
        {
            deckCleanup = new Dictionary<ISkill, List<DeckType>>();
            markCleanup = new Dictionary<ISkill, List<PlayerAttribute>>();
        }

        public void CalldownCleanupCrew(ISkill skill, DeckType deck)
        {
            if (!deckCleanup.ContainsKey(skill)) deckCleanup.Add(skill, new List<DeckType>());
            deckCleanup[skill].Add(deck);
        }

        public void CalldownCleanupCrew(ISkill skill, PlayerAttribute attr)
        {
            if (!markCleanup.ContainsKey(skill)) markCleanup.Add(skill, new List<PlayerAttribute>());
            markCleanup[skill].Add(attr);
        }

        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (gameEvent == GameEvent.PlayerSkillSetChanged)
            {
                SkillSetChangedEventArgs args = eventArgs as SkillSetChangedEventArgs;
                Trace.Assert(args != null);
                if (!args.IsLosingSkill) return;
                foreach (var sk in args.Skills)
                {
                    if (deckCleanup.ContainsKey(sk))
                    {
                        foreach (var deck in deckCleanup[sk])
                        {
                            if (Game.CurrentGame.Decks[args.Source, deck].Count > 0)
                            {
                                List<Card> toDiscard = new List<Card>(Game.CurrentGame.Decks[args.Source, deck]);
                                if (toDiscard.Any(c => c.Type.IsCardCategory(CardCategory.Hero)))
                                {
                                    //HuaShenDeck
                                    CardsMovement move = new CardsMovement();
                                    move.Cards = toDiscard;
                                    move.To = new DeckPlace(null, DeckType.Heroes);
                                    Game.CurrentGame.MoveCards(move);
                                }
                                else
                                {
                                    Game.CurrentGame.HandleCardDiscard(args.Source, toDiscard);
                                }
                            }
                        }
                    }
                    if (markCleanup.ContainsKey(sk))
                    {
                        foreach (var player in Game.CurrentGame.Players)
                        {
                            foreach (var mark in markCleanup[sk])
                            {
                                player[mark] = 0;
                            }
                        }
                    }
                }
            }
        }
    }
}
