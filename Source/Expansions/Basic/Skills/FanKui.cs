using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 反馈-每当你受到一次伤害后，你可以获得伤害来源的一张牌。
    /// </summary>
    public class FanKui : PassiveSkill
    {
        class FanKuiTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source == null || eventArgs.Targets.IndexOf(Owner) < 0)
                {
                    return;
                }
                int answer = 0;
                if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("FanKui", eventArgs.Source), Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    List<DeckPlace> deck = new List<DeckPlace>();
                    deck.Add(new DeckPlace(eventArgs.Source, DeckType.Hand));
                    deck.Add(new DeckPlace(eventArgs.Source, DeckType.Equipment));
                    List<int> max = new List<int>();
                    max.Add(1);
                    List<List<Card>> result;
                    List<string> deckname = new List<string>();
                    deckname.Add("FanKui choice");
                    Card theCard;
                    if (Game.CurrentGame.Decks[eventArgs.Source, DeckType.Hand].Count == 0 &&
                        Game.CurrentGame.Decks[eventArgs.Source, DeckType.Equipment].Count == 0)
                    {
                        return;
                    }
                    if (!Game.CurrentGame.UiProxies[eventArgs.Source].AskForCardChoice(new CardChoicePrompt("FanKui"), deck, deckname, max, new RequireOneCardChoiceVerifier(), out result, new List<bool>() { false }))
                    {

                        Trace.TraceInformation("Invalid choice for FanKui");
                        theCard = Game.CurrentGame.Decks[eventArgs.Source, DeckType.Hand]
                            .Concat(Game.CurrentGame.Decks[eventArgs.Source, DeckType.Equipment]).First();
                    }
                    else
                    {
                        theCard = result[0][0];
                    }
                    List<Card> cards = new List<Card>();
                    cards.Add(theCard);
                    Game.CurrentGame.HandleCardTransferToHand(eventArgs.Source, Owner, cards);
                }
            }
            
            public FanKuiTrigger(Player p)
            {
                Owner = p;
            }
        }

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            Game.CurrentGame.RegisterTrigger(GameEvent.AfterDamageInflicted, new FanKuiTrigger(owner));
        }
    }
}
