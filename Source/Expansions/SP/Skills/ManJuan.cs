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

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>
    /// 漫卷-每当你将获得任何一张牌，将之置于弃牌堆。若此情况处于你的回合中，你可依次将与该牌点数相同的一张牌从弃牌堆置于你手上。
    /// </summary>
    public class ManJuan : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            DeckType mjDeck = new DeckType("ManJuan");
            List<Card> theCards = new List<Card>(eventArgs.Cards);
            CardsMovement move = new CardsMovement();
            move.Cards = eventArgs.Cards;
            move.To = new DeckPlace(null, DeckType.Discard);
            Game.CurrentGame.MoveCards(move);
            if (Game.CurrentGame.CurrentPlayer == Owner)
            {
                foreach (var reclaim in theCards)
                {
                    List<Card> garbageList = new List<Card>();
                    foreach (var garbage in Game.CurrentGame.Decks[null, DeckType.Discard])
                    {
                        if (garbage.Rank == reclaim.Rank)
                        {
                            garbageList.Add(garbage);
                        }
                    }
                    move = new CardsMovement();
                    move.Cards = garbageList;
                    move.To = new DeckPlace(null, mjDeck);
                    Game.CurrentGame.MoveCards(move);

                    IUiProxy ui = Game.CurrentGame.UiProxies[Owner];
                    List<List<Card>> answer;

                    if (!ui.AskForCardChoice(new CardChoicePrompt("ManJuan"), new List<DeckPlace>() {new DeckPlace(null, mjDeck)}, new List<string>() {"ZuiXiang"}, new List<int>() {1}, new RequireOneCardChoiceVerifier(), out answer))
                    {
                        Trace.TraceInformation("Player {0} Invalid answer", Owner);
                        answer = new List<List<Card>>();
                        answer.Add(new List<Card>());
                        answer[0].Add(Game.CurrentGame.Decks[null, mjDeck][0]);
                    }
                    Trace.Assert(answer.Count == 1 && answer[0].Count == 1);

                    move = new CardsMovement();
                    move.Cards = new List<Card>(answer[0]);
                    move.To = new DeckPlace(Owner, DeckType.Hand);
                    Game.CurrentGame.MoveCards(move);
                    move = new CardsMovement();
                    move.Cards = new List<Card>(Game.CurrentGame.Decks[null, mjDeck]);
                    move.To = new DeckPlace(null, DeckType.Discard);
                    Game.CurrentGame.MoveCards(move);
                }
            }
        }

        public ManJuan()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.CardsAcquired, trigger);
            IsEnforced = true;
        }
    }
}