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
    /// 观星-你可以看星星
    /// </summary>
    public class GuanXing : TriggerSkill
    {
        class GuanXingVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                Dictionary<Card, bool> bucket = new Dictionary<Card, bool>();
                if (answer != null)
                {
                    if (answer.Count > 0)
                    {
                        foreach (Card c in answer[0])
                        {
                            if (bucket.ContainsKey(c))
                                return VerifierResult.Fail;
                            bucket.Add(c, true);
                        }
                    }
                    if (answer.Count > 1)
                    {
                        foreach (Card c in answer[1])
                        {
                            if (bucket.ContainsKey(c))
                                return VerifierResult.Fail;
                            bucket.Add(c, true);
                        }
                    }
                }
                foreach (Card c in cards)
                {
                    if (!bucket.ContainsKey(c))
                    {
                        return VerifierResult.Fail;
                    }
                }
                return VerifierResult.Success;
            }

            List<Card> cards;
            public GuanXingVerifier(List<Card> c)
            {
                cards = new List<Card>(c);
            }
            public UiHelper Helper
            {
                get { return null; }
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            DeckType GuanXingDeck = new DeckType("GuanXing");

            CardsMovement move = new CardsMovement();
            move.cards = new List<Card>();
            int toDraw = Math.Min(5, Game.CurrentGame.AlivePlayers.Count);
            for (int i = 0; i < toDraw; i++)
            {
                Game.CurrentGame.SyncImmutableCard(Owner, Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                move.cards.Add(c);
            }
            move.to = new DeckPlace(null, GuanXingDeck);
            Game.CurrentGame.MoveCards(move, null);
            List<List<Card>> answer;
            AdditionalCardChoiceOptions options = new AdditionalCardChoiceOptions();
            options.Rearrangeable = new List<bool>() { true, true };
            options.DefaultResult = new List<List<Card>>() { new List<Card>(Game.CurrentGame.Decks[null, GuanXingDeck]), new List<Card>() };
            if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("GuanXing"),
                    new List<DeckPlace>() { },
                    new List<string>() { "PaiDuiDing", "PaiDuiDi" },
                    new List<int>() { toDraw, toDraw },
                    new GuanXingVerifier(Game.CurrentGame.Decks[null, GuanXingDeck]),
                    out answer, 
                    options,
                    CardChoiceCallback.GenericCardChoiceCallback))
            {
                Game.CurrentGame.InsertBeforeDeal(null, Game.CurrentGame.Decks[null, GuanXingDeck]);
            }
            else
            {
                Game.CurrentGame.InsertBeforeDeal(null, answer[0]);
                Game.CurrentGame.PlaceIntoDiscard(null, answer[1]);
            }

        }


        public GuanXing()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAutoInvoked = true;
        }

    }
}
