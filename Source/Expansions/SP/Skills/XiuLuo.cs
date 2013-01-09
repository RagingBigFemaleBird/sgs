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
    /// 修罗-回合开始阶段开始时，你可以弃置与你判定区里一张牌相同花色的一张手牌，然后你弃置你判定区里的该牌。
    /// </summary>
    public class XiuLuo : TriggerSkill
    {
        class XiuLuoVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                if ((answer != null && answer.Count > 2) ||
                    (answer != null && (answer[0] != null && answer[0].Count > 1 || answer[1] != null && answer[0].Count > 1)))
                {
                    return VerifierResult.Fail;
                }
                if (answer != null && answer.Count > 0)
                {
                    if (answer[0] != null && answer[0].Count != 0 && answer[0][0].Place.DeckType == DeckType.Hand)
                    {
                        return VerifierResult.Fail;
                    }
                    if (answer[1] != null && answer[1].Count != 0 && answer[1][0].Place.DeckType == DeckType.DelayedTools)
                    {
                        return VerifierResult.Fail;
                    }
                }
                if (answer[0] == null || answer[0].Count == 0 || answer[1] == null || answer[1].Count == 0)
                {
                    return VerifierResult.Partial;
                }
                Card fromDelayed = answer[0][0];
                Card fromHand = answer[1][0];
                if (fromHand != null && fromDelayed != null && fromHand.Suit != fromDelayed.Suit)
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
            public UiHelper Helper
            {
                get { return null; }
            }
        }

        void Run(Player owner, GameEvent gameEvent, GameEventArgs args)
        {
            List<List<Card>> answer;
            List<DeckPlace> sourceDeck = new List<DeckPlace>();
            sourceDeck.Add(new DeckPlace(owner, DeckType.DelayedTools));
            sourceDeck.Add(new DeckPlace(owner, DeckType.Hand));
            if (owner.AskForCardChoice(new CardChoicePrompt("XiuLuo", owner),
                sourceDeck,
                new List<string>() { "XLJinNang", "XLShouPai" },
                new List<int>() { 1, 1 },
                new XiuLuoVerifier(),
                out answer,
                null,
                CardChoiceCallback.GenericCardChoiceCallback))
            {
                NotifySkillUse();
                Game.CurrentGame.HandleCardDiscard(owner, answer[1]);
                Game.CurrentGame.HandleCardDiscard(owner, answer[0]);
            }
        }

        public XiuLuo()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p.DelayedTools().Count > 0 && p.HandCards().Count > 0; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAutoInvoked = null;
        }
    }
}
