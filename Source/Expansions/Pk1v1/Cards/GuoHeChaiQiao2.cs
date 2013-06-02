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

namespace Sanguosha.Expansions.Pk1v1.Cards
{

    public class GuoHeChaiQiao2 : CardHandler
    {
        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            if (source.IsDead) return;
            if (dest.HandCards().Count + dest.Equipments().Count == 0) return; // ShunChai -> WuXie(from target) -> WuXie(soemone else) -> target has no card
            List<string> resultDeckPlace = new List<string>();
            resultDeckPlace.Add("GuoHeChaiQiao");
            List<int> resultDeckMax = new List<int>();
            resultDeckMax.Add(1);
            List<List<Card>> answer;
            bool doHandCard = true;
            if (dest.Equipments().Count != 0)
            {
                int result = 0;
                source.AskForMultipleChoice(new MultipleChoicePrompt("GuoHeChaiQiao2"), new List<OptionPrompt>() { new OptionPrompt("ShouPai"), new OptionPrompt("ZhuangBeiPai") }, out result);
                if (result == 1) doHandCard = false;
            }
            if (doHandCard)
            {
                Game.CurrentGame.SyncImmutableCards(source, Game.CurrentGame.Decks[dest, DeckType.Hand]);
                Game.CurrentGame.HandCardVisibility[source].Add(dest);
                var places = new List<DeckPlace>() { new DeckPlace(dest, DeckType.Hand) };
                if (!source.AskForCardChoice(new CardChoicePrompt("GuoHeChaiQiao2"), places, resultDeckPlace, resultDeckMax, new RequireOneCardChoiceVerifier(true), out answer))
                {
                    Trace.TraceInformation("Player {0} Invalid answer", source.Id);
                    answer = new List<List<Card>>();
                    answer.Add(Game.CurrentGame.PickDefaultCardsFrom(places));
                }
                foreach (Card c in dest.HandCards()) Game.CurrentGame.HideHandCard(c);
                Game.CurrentGame.HandCardVisibility[source].Remove(dest);
                Game.CurrentGame.HandleCardDiscard(dest, answer[0]);
            }
            else
            {
                var places = new List<DeckPlace>() { new DeckPlace(dest, DeckType.Equipment) };
                if (!source.AskForCardChoice(new CardChoicePrompt("GuoHeChaiQiao2"), places, resultDeckPlace, resultDeckMax, new RequireOneCardChoiceVerifier(true), out answer))
                {
                    Trace.TraceInformation("Player {0} Invalid answer", source.Id);
                    answer = new List<List<Card>>();
                    answer.Add(Game.CurrentGame.PickDefaultCardsFrom(places));
                }
                Game.CurrentGame.HandleCardDiscard(dest, answer[0]);

            }
        }

        public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
        {
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (!isLooseVerify && targets.Count > 1)
            {
                return VerifierResult.Fail;
            }

            foreach (var player in targets)
            {
                if (player == source)
                {
                    return VerifierResult.Fail;
                }
                if (Game.CurrentGame.Decks[player, DeckType.Hand].Count == 0 &&
                    Game.CurrentGame.Decks[player, DeckType.Equipment].Count == 0)
                {
                    return VerifierResult.Fail;
                }
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }
}
