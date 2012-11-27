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
    
    public abstract class ShunChai : CardHandler
    {
        protected abstract string ResultDeckName {get;}

        protected abstract string ChoicePrompt {get;}

        protected abstract DeckPlace ShunChaiDest(Player source, Player dest);

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
            IUiProxy ui = Game.CurrentGame.UiProxies[source];
            if (source.IsDead) return;
            List<DeckPlace> places = new List<DeckPlace>();
            places.Add(new DeckPlace(dest, DeckType.Hand));
            places.Add(new DeckPlace(dest, DeckType.Equipment));
            places.Add(new DeckPlace(dest, DeckType.DelayedTools));
            List<string> resultDeckPlace = new List<string>();
            resultDeckPlace.Add(ResultDeckName);
            List<int> resultDeckMax = new List<int>();
            resultDeckMax.Add(1);
            List<List<Card>> answer;
            int windowId = 0;
            if (!ui.AskForCardChoice(new CardChoicePrompt(ChoicePrompt), places, resultDeckPlace, resultDeckMax, new RequireOneCardChoiceVerifier(), out answer, new List<bool>() { false }, ref windowId))
            {
                Trace.TraceInformation("Player {0} Invalid answer", source.Id);
                answer = new List<List<Card>>();
                answer.Add(new List<Card>());
                var collection = Game.CurrentGame.Decks[dest, DeckType.Hand].Concat
                                 (Game.CurrentGame.Decks[dest, DeckType.DelayedTools].Concat
                                 (Game.CurrentGame.Decks[dest, DeckType.Equipment]));
                answer[0].Add(collection.First());
            }
            Card theCard = answer[0][0];
            Game.CurrentGame.SyncCardAll(ref theCard);
            Trace.Assert(answer.Count == 1 && answer[0].Count == 1);

            if (ShunChaiDest(source, dest).DeckType == DeckType.Discard)
            {
                Game.CurrentGame.HandleCardDiscard(dest, new List<Card>() { theCard });
            }
            else
            {
                Game.CurrentGame.HandleCardTransferToHand(dest, source, new List<Card>() { theCard });
            }
        }

        protected abstract bool ShunChaiAdditionalCheck(Player source, Player dest, ICard card);

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            Player player = targets[0];
            if (player == source)
            {
                return VerifierResult.Fail;
            }
            if (!ShunChaiAdditionalCheck(source, player, card))
            {
                return VerifierResult.Fail;
            }
            if (Game.CurrentGame.Decks[player, DeckType.Hand].Count == 0 &&
                Game.CurrentGame.Decks[player, DeckType.DelayedTools].Count == 0 &&
                Game.CurrentGame.Decks[player, DeckType.Equipment].Count == 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }
}
