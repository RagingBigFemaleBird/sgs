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

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 攻心-出牌阶段，你可以观看一名其他角色的手牌，并可以展示其中一张红桃牌，然后将其弃置或置于牌堆顶。每阶段限一次。
    /// </summary>
    public class GongXin : ActiveSkill
    {
        public GongXin()
        {
            Helper.RevealCards = true;
        }
        private static PlayerAttribute GongXinUsed = PlayerAttribute.Register("GongXinUsed", true);
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[GongXinUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets.Count > 0 && (arg.Targets[0].HandCards().Count == 0))
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[GongXinUsed] = 1;
            Player target = arg.Targets[0];
            List<List<Card>> answer;
            Game.CurrentGame.SyncImmutableCards(Owner, Game.CurrentGame.Decks[target, DeckType.Hand]);
            Game.CurrentGame.HandCardVisibility[Owner].Add(target);
            if (Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("GongXin", Owner),
                    new List<DeckPlace>() { new DeckPlace(target, DeckType.Hand) },
                    new List<string>() { "PaiDuiDing", "QiPaiDui" },
                    new List<int>() { 1, 1 },
                    new GongXinVerifier(),
                    out answer,
                    null,
                    CardChoiceCallback.GenericCardChoiceCallback))
            {
                foreach (Card c in target.HandCards()) Game.CurrentGame.HideHandCard(c);
                Game.CurrentGame.HandCardVisibility[Owner].Remove(target);
                if (answer[0] != null && answer[0].Count > 0)
                {
                    var theCard = answer[0][0];
                    Game.CurrentGame.SyncCardAll(ref theCard);
                    Game.CurrentGame.NotificationProxy.NotifyLogEvent(new LogEvent("GongXin1", Owner, target, theCard), new List<Player>() { Owner, target });
                    Game.CurrentGame.InsertBeforeDeal(target, new List<Card>() { theCard });
                }
                else if (answer[1] != null && answer[1].Count > 0)
                {
                    var theCard = answer[1][0];
                    Game.CurrentGame.SyncCardAll(ref theCard);
                    Game.CurrentGame.NotificationProxy.NotifyLogEvent(new LogEvent("GongXin2", Owner, target, theCard), new List<Player>() { Owner, target });
                    Game.CurrentGame.PlaceIntoDiscard(target, new List<Card>() { theCard });
                }
            }
            else
            {
                foreach (Card c in target.HandCards()) Game.CurrentGame.HideHandCard(c);
                Game.CurrentGame.HandCardVisibility[Owner].Remove(target);
            }
            return true;
        }
        class GongXinVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                if (answer == null) return VerifierResult.Success;
                Trace.Assert(answer.Count == 2);
                var result = answer[0].Concat(answer[1]);
                if (result.Count() == 0 || (result.Count() == 1 && result.First().Suit == SuitType.Heart))
                    return VerifierResult.Success;
                else
                    return VerifierResult.Fail;
            }
            public UiHelper Helper
            {
                get { return new UiHelper() { RevealCards = true }; }
            }
        }
    }
}
