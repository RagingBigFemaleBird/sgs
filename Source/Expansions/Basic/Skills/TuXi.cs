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

            public override IList<CardHandler> AcceptableCardType
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
                Game.CurrentGame.EnterAtomicContext();
                foreach (Player p in players)
                {
                    List<List<Card>> answer;
                    if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("TuXi"), new List<DeckPlace>() { new DeckPlace(p, DeckType.Hand) },
                        new List<string>() { "TuXi" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(), out answer, new List<bool>() { false }))
                    {
                        answer = new List<List<Card>>();
                        answer.Add(new List<Card>());
                        answer[0].Add(Game.CurrentGame.Decks[p, DeckType.Hand][0]);
                    }
                    Game.CurrentGame.HandleCardTransferToHand(p, Owner, answer[0]);
                }
                Game.CurrentGame.ExitAtomicContext();

                throw new TriggerResultException(TriggerResult.Skip);
            }
        }


        public TuXi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return true; },
                GetTheirCards,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, Priority = SkillPriority.TuXi };
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger);
        }
    }
}
