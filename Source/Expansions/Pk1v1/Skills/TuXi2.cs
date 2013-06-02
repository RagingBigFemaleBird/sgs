using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Pk1v1.Skills
{
    /// <summary>
    /// 突袭-摸牌阶段，你可以少摸一张牌，改为获得对手一张手牌。
    /// </summary>
    public class TuXi2 : TriggerSkill
    {
        class TuXiVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || (cards != null && cards.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (players == null || players.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                foreach (Player p in players)
                {
                    if (p == source)
                    {
                        return VerifierResult.Fail;
                    }
                    if (Game.CurrentGame.Decks[p, DeckType.Hand].Count == 0)
                    {
                        return VerifierResult.Fail;
                    }
                    if (p.HandCards().Count <= source.HandCards().Count) return VerifierResult.Fail;
                }
                if (players.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }

            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return new List<CardHandler>(); }
            }
        }

        void GetTheirCards(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("TuXi2"), new TuXiVerifier(), out skill, out cards, out players))
            {
                Game.CurrentGame.SortByOrderOfComputation(Game.CurrentGame.CurrentPlayer, players);
                NotifySkillUse(players);
                var p = players[0];
                List<List<Card>> answer;
                if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("TuXi"), new List<DeckPlace>() { new DeckPlace(p, DeckType.Hand) },
                    new List<string>() { "TuXi" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(true), out answer))
                {
                    answer = new List<List<Card>>();
                    answer.Add(Game.CurrentGame.PickDefaultCardsFrom(new List<DeckPlace>() { new DeckPlace(p, DeckType.Hand) }));
                }
                CardsMovement move = new CardsMovement();
                move.Cards = new List<Card>(answer[0]);
                move.To = new DeckPlace(Owner, DeckType.Hand);

                Game.CurrentGame.MoveCards(move);
                Owner[Player.DealAdjustment]--;
            }
        }


        public TuXi2()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                GetTheirCards,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Draw], trigger);
            IsAutoInvoked = null;
        }
    }
}
