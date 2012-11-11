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

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 鬼道-在一名角色的判定牌生效前，你可以用一张黑色牌替换之。
    /// </summary>
    public class GuiDao : TriggerSkill
    {
        public void OnJudgeBegin(Player player, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (Game.CurrentGame.Decks[player, DeckType.Hand].Count == 0)
            {
                return;
            }
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Game.CurrentGame.UiProxies[player].AskForCardUsage(new CardUsagePrompt("GuiDao", eventArgs.Source, eventArgs.Card.Suit, eventArgs.Card.Rank), new GuiDaoVerifier(), out skill, out cards, out players))
            {
                Game.CurrentGame.EnterAtomicContext();
                List<Card> toDiscard = new List<Card>(Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult]);
                eventArgs.Card = new ReadOnlyCard(cards[0]);
                eventArgs.Cards = new List<Card>() { cards[0] };
                CardsMovement move = new CardsMovement();
                move.cards = new List<Card>();
                move.cards.AddRange(cards);
                move.to = new DeckPlace(eventArgs.Source, DeckType.JudgeResult);
                Game.CurrentGame.MoveCards(move, null);
                Game.CurrentGame.PlayerLostCard(player, cards);
                Game.CurrentGame.HandleCardTransferToHand(null, player, toDiscard);
                Game.CurrentGame.ExitAtomicContext();
            }
        }


        public class GuiDaoVerifier : ICardUsageVerifier
        {

            public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || (players != null && players.Count > 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards[0].SuitColor != SuitColorType.Black)
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }

            public IList<CardHandler> AcceptableCardType
            {
                get { return null; }
            }

            public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return FastVerify(source, skill, cards, players);
            }

            public UiHelper Helper
            {
                get { return new UiHelper(); }
            }
        }

        public GuiDao()
        {
            Triggers.Add(GameEvent.PlayerJudgeBegin, new RelayTrigger(OnJudgeBegin, TriggerCondition.Global));
        }
    }
}
