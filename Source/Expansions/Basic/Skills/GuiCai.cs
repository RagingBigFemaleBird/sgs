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
    /// 鬼才-在一名角色的判定牌生效前，你可以打出一张手牌代替之。
    /// </summary>
    public class GuiCai : TriggerSkill
    {

        public void ReplaceJudgementCard(Player player, Player judgePlayer, Card card, ISkill skill)
        {
            card.Log = new ActionLog();
            card.Log.Source = player;
            card.Log.SkillAction = skill;
            card.Log.GameAction = GameAction.ReplaceJudge;
            Game.CurrentGame.EnterAtomicContext();
            var judgeDeck = Game.CurrentGame.Decks[judgePlayer, DeckType.JudgeResult];
            List<Card> toDiscard = new List<Card>() {judgeDeck.Last()};
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>() {card};
            move.To = new DeckPlace(judgePlayer, DeckType.JudgeResult);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerLostCard(card.Place.Player, new List<Card>() {card});
            Game.CurrentGame.HandleCardDiscard(judgePlayer, toDiscard, DiscardReason.Judge);
            Game.CurrentGame.ExitAtomicContext();
        }

        protected virtual void OnJudgeBegin(Player player, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (Game.CurrentGame.Decks[player, DeckType.Hand].Count == 0)
            {
                return;
            }
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            Card c = Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult].Last();
            if (Game.CurrentGame.UiProxies[player].AskForCardUsage(new CardUsagePrompt("GuiCai", eventArgs.Source, c.Suit, c.Rank), new GuiCaiVerifier(), out skill, out cards, out players))
            {
                NotifySkillUse(new List<Player>());
                ReplaceJudgementCard(player, eventArgs.Source, cards[0], this);
            }
        }
 

        public class GuiCaiVerifier : ICardUsageVerifier
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
                if (cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
                if (!Game.CurrentGame.PlayerCanPlayCard(source, cards[0]))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }

            public IList<CardHandler> AcceptableCardTypes
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

        public GuiCai()
        {
            Triggers.Add(GameEvent.PlayerJudgeBegin, new RelayTrigger(OnJudgeBegin, TriggerCondition.Global));
            IsAutoInvoked = null;
        }
    }
}
