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
using System.Threading;
using Sanguosha.Core.Utils;

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{

    /// <summary>
    /// 心战-出牌阶段，若你的手牌数大于你的体力上限，你可以：观看牌堆顶的三张牌，然后亮出其中任意数量的红桃牌并获得之，其余以任意顺序置于牌堆顶。每阶段限一次。
    /// </summary>
    public class XinZhan : ActiveSkill
    {
        public XinZhan()
        {
            Helper.HasNoConfirmation = true;
        }
        private static int choiceCount = 3;
        private static PlayerAttribute XinZhanUsed = PlayerAttribute.Register("XinZhanUsed", true);
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[XinZhanUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            if (Owner.HandCards().Count() <= Owner.MaxHealth)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[XinZhanUsed] = 1;
            DeckType XinZhanDeck = new DeckType("XinZhan");
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>();
            for (int i = 0; i < choiceCount; i++)
            {
                Game.CurrentGame.SyncImmutableCard(Owner, Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                move.Cards.Add(c);
            }
            move.To = new DeckPlace(null, XinZhanDeck);
            move.Helper.IsFakedMove = true;
            Game.CurrentGame.MoveCards(move, false, GameDelayTypes.None);
            List<List<Card>> answer;
            AdditionalCardChoiceOptions options = new AdditionalCardChoiceOptions();
            options.Rearrangeable = new List<bool>() { true, false };
            options.DefaultResult = new List<List<Card>>() { new List<Card>(Game.CurrentGame.Decks[null, XinZhanDeck]), new List<Card>() };
            if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("XinZhan", Owner),
                    new List<DeckPlace>() { },
                    new List<string>() { "PaiDuiDing", "HuoDe" },
                    new List<int>() { choiceCount, choiceCount },
                    new XinZhanVerifier(),
                    out answer,
                    options,
                    CardChoiceCallback.GenericCardChoiceCallback))
            {
                Game.CurrentGame.NotificationProxy.NotifyLogEvent(new LogEvent("XinZhan", Owner, Game.CurrentGame.Decks[null, XinZhanDeck].Count), new List<Player>() { Owner }, false);
                Game.CurrentGame.InsertBeforeDeal(null, Game.CurrentGame.Decks[null, XinZhanDeck], new MovementHelper() { IsFakedMove = true });
            }
            else
            {
                Game.CurrentGame.NotificationProxy.NotifyLogEvent(new LogEvent("XinZhan", Owner, answer[0].Count), new List<Player>() { Owner }, false);
                Game.CurrentGame.InsertBeforeDeal(null, answer[0], new MovementHelper() { IsFakedMove = true });
                Game.CurrentGame.HandleCardTransferToHand(null, Owner, answer[1]);
            }
            return true;
        }

        class XinZhanVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                foreach (Card c in answer[1])
                    if (c.Suit != SuitType.Heart)
                        return VerifierResult.Fail;
                return VerifierResult.Success;
            }
            static UiHelper helper = new UiHelper() { AdditionalFineGrainedCardChoiceRevealPolicy = new List<bool>() { false, true }, ShowToAll = true };
            public UiHelper Helper
            {
                get { return helper; }
            }
        }
    }
}
