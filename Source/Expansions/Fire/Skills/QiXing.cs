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

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 七星-分发启始手牌时，共发你十一张牌，选四张作为手牌，其余的面朝下置于一旁，称为“星”。每当你于摸牌阶段摸牌后，可用任意数量的手牌等量替换这些“星”。
    /// </summary>
    public class QiXing : TriggerSkill
    {
        class QiXingVerifier : CardsAndTargetsVerifier
        {
            public QiXingVerifier(int QiXingCount)
            {
                MaxPlayers = 0;
                MinPlayers = 0;
                MaxCards = QiXingCount;
                MinCards = QiXingCount;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return true;
            }

            public override UiHelper Helper
            {
                get
                {
                    return new UiHelper() { NoCardReveal = true };
                }
            }
        }

        void GameStart(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            // hack the cards to owner's hand. do not trigger anything
            List<Card> additionalCards = new List<Card>();
            for (int i = 0; i < 7; i++)
            {
                Game.CurrentGame.SyncImmutableCard(Owner, Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                additionalCards.Add(c);
            }
            CardsMovement move = new CardsMovement();
            move.cards = new List<Card>(additionalCards);
            move.to = new DeckPlace(Owner, DeckType.Hand);
            Game.CurrentGame.MoveCards(move, new MovementHelper() { IsFakedMove = true });
            if (!Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("QiXing"), new QiXingVerifier(7), out skill, out cards, out players))
            {
                cards = new List<Card>();
                cards.AddRange(Game.CurrentGame.Decks[Owner, DeckType.Hand].GetRange(0, 7));
            }
            move.cards = new List<Card>(cards);
            move.to = new DeckPlace(Owner, QiXingDeck);
            Game.CurrentGame.MoveCards(move, null);
        }

        void AfterDraw(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            int qxCount = Game.CurrentGame.Decks[Owner, QiXingDeck].Count;
            // hack the cards to owner's hand. do not trigger anything
            CardsMovement move = new CardsMovement();
            move.cards = new List<Card>(Game.CurrentGame.Decks[Owner, QiXingDeck]);
            move.to = new DeckPlace(Owner, DeckType.Hand);
            Game.CurrentGame.MoveCards(move, new MovementHelper() { IsFakedMove = true });
            if (!Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("QiXing"), new QiXingVerifier(qxCount), out skill, out cards, out players))
            {
                cards = new List<Card>();
                cards.AddRange(Game.CurrentGame.Decks[Owner, DeckType.Hand].GetRange(0, qxCount));
            }
            move.cards = new List<Card>(cards);
            move.to = new DeckPlace(Owner, QiXingDeck);
            Game.CurrentGame.MoveCards(move, null);
        }

        static PrivateDeckType QiXingDeck = new PrivateDeckType("QiXing");

        public QiXing()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                GameStart,
                TriggerCondition.OwnerIsSource
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                AfterDraw,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger);
            Triggers.Add(GameEvent.PhaseEndEvents[TurnPhase.Draw], trigger2);
            IsAutoInvoked = true;
        }

    }
}
