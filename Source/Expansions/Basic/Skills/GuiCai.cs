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
    public class GuiCai : PassiveSkill
    {
        class GuiCaiTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (Game.CurrentGame.Decks[Owner, DeckType.Hand].Count == 0)
                {
                    return;
                }
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("GuiCai", eventArgs.Source, eventArgs.Card.Suit, eventArgs.Card.Rank), new GuiCaiVerifier(), out skill, out cards, out players))
                {
                    Game.CurrentGame.EnterAtomicContext();
                    List<Card> toDiscard = new List<Card>(Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult]);
                    eventArgs.Card = cards[0];
                    CardsMovement move = new CardsMovement();
                    move.cards = new List<Card>();
                    move.cards.AddRange(cards);
                    move.to = new DeckPlace(eventArgs.Source, DeckType.JudgeResult);
                    Game.CurrentGame.MoveCards(move, null);
                    Game.CurrentGame.PlayerLostCard(Owner, cards);
                    Game.CurrentGame.HandleCardDiscard(eventArgs.Source, toDiscard, DiscardReason.Judge);
                    Game.CurrentGame.ExitAtomicContext();
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

            public GuiCaiTrigger(Player p)
            {
                Owner = p;
            }
        }

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            Game.CurrentGame.RegisterTrigger(GameEvent.PlayerJudgeBegin, new GuiCaiTrigger(owner));
        }
    }
}
