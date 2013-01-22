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
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 醇醪-回合结束阶段开始时，若你的武将牌上没有牌，你可以将任意数量的【杀】置于你的武将牌上，称为“醇”；当一名角色处于濒死状态时，你可以将一张“醇”置入弃牌堆，视为该角色使用一张【酒】。
    /// </summary>
    public class ChunLao : TriggerSkill
    {
        class ChunLaoStoreChunVerifier : CardsAndTargetsVerifier
        {
            public ChunLaoStoreChunVerifier()
            {
                MinCards = 1;
                MaxCards = int.MaxValue;
                MinPlayers = 0;
                MaxPlayers = 0;
                Discarding = false;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand && card.Type is Sha;
            }
        }

        public void StoreChun(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (owner.AskForCardUsage(new CardUsagePrompt("ChunLao"), new ChunLaoStoreChunVerifier(), out skill, out cards, out players))
            {
                NotifySkillUse();
                CardsMovement move = new CardsMovement();
                move.Cards = cards;
                move.To = new DeckPlace(Owner, ChunDeck);
                Game.CurrentGame.MoveCards(move);
            }
        }

        class ChuLaoVerifier : CardsAndTargetsVerifier
        {
            public ChuLaoVerifier()
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
                Discarding = true;
                Helper.OtherDecksUsed.Add(ChunDeck);
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == ChunDeck;
            }
        }

        public void SaveALife(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            while (eventArgs.Source.Health <= 0 && Game.CurrentGame.Decks[owner, ChunDeck].Count > 0 &&
                owner.AskForCardUsage(new CardUsagePrompt("ChunLaoSave"), new  ChuLaoVerifier(), out skill, out cards, out players))
            {
                NotifySkillUse(eventArgs.Targets);
                Card theCard = cards[0];
                Game.CurrentGame.HandleCardDiscard(owner, new List<Card>() { theCard });
                GameEventArgs args = new GameEventArgs();
                args.Source = eventArgs.Source;
                args.Targets = new List<Player>() { eventArgs.Source };
                args.Skill = new ChunLaoJiuCardTransformSkill();
                args.Cards = new List<Card>();
                Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
            }
        }

        private class ChunLaoJiuCardTransformSkill : CardTransformSkill
        {
            public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
            {
                Trace.Assert(cards == null || cards.Count == 0);
                card = new CompositeCard();
                card.Type = new Jiu();
                return VerifierResult.Success;
            }
            protected override void NotifyAction(Player source, List<Player> targets, CompositeCard cards)
            {
            }
        }

        public ChunLao()
        {
            var trigger1 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return Game.CurrentGame.Decks[p, ChunDeck].Count == 0; },
                    StoreChun,
                    TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false, IsAutoNotify = false };

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return Game.CurrentGame.Decks[p, ChunDeck].Count != 0 && a.Source.Health <= 0; },
                    SaveALife,
                    TriggerCondition.OwnerIsTarget
                ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger1);
            Triggers.Add(GameEvent.PlayerDying, trigger2);
            IsAutoInvoked = null;
            ExtraCardsDeck = ChunDeck;
        }

        public static PrivateDeckType ChunDeck = new PrivateDeckType("Chun", false);
    }
}
