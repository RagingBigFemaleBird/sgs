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
    /// 突袭-摸牌阶段，你可以放弃摸牌，改为获得一至两名其他角色的各一张手牌。
    /// </summary>
    public class TuXi : TriggerSkill
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
                }
                if (players.Count > 2)
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
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("TuXi"), new TuXiVerifier(), out skill, out cards, out players))
            {
                Game.CurrentGame.SortByOrderOfComputation(Game.CurrentGame.CurrentPlayer, players);
                NotifySkillUse(players);
                StagingDeckType TuXiDeck = new StagingDeckType("TiXi");
                CardsMovement move = new CardsMovement();
                move.Helper.IsFakedMove = true;
                foreach (Player p in players)
                {
                    if (p.HandCards().Count == 0) continue;
                    List<List<Card>> answer;
                    if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("TuXi"), new List<DeckPlace>() { new DeckPlace(p, DeckType.Hand) },
                        new List<string>() { "TuXi" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(true), out answer))
                    {
                        answer = new List<List<Card>>();
                        answer.Add(new List<Card>());
                        answer[0].Add(Game.CurrentGame.Decks[p, DeckType.Hand][0]);
                    }
                    move.Cards = answer[0];
                    move.To = new DeckPlace(p, TuXiDeck);
                    Game.CurrentGame.MoveCards(move, false, Core.Utils.GameDelayTypes.None);
                    Game.CurrentGame.PlayerLostCard(p, answer[0]);
                }
                move.Cards.Clear();
                move.Helper.IsFakedMove = false;
                move.To = new DeckPlace(Owner, DeckType.Hand);
                foreach (Player p in players)
                {
                    move.Cards.AddRange(Game.CurrentGame.Decks[p, TuXiDeck]);
                }
                cards = new List<Card>(move.Cards);
                Game.CurrentGame.MoveCards(move);
                Game.CurrentGame.PlayerAcquiredCard(Owner, cards);
                Game.CurrentGame.CurrentPhaseEventIndex++;
                Game.CurrentGame.NotificationProxy.NotifyActionComplete();
                throw new TriggerResultException(TriggerResult.End);
            }
        }


        public TuXi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                GetTheirCards,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, Priority = SkillPriority.TuXi, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Draw], trigger);
            IsAutoInvoked = null;
        }
    }
}
