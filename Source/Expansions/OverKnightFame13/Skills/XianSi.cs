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

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    /// <summary>
    /// 陷嗣 - 回合开始阶段开始时，你可以弃置一张手牌，将至多两名角色的各一张牌移动至你的武将牌上，称为“逆”。每当其他角色需要对你使用一张【杀】时，该角色可以弃置你的一张“逆”，视为对你使用一张【杀】。
    /// </summary>
    public class XianSi : TriggerSkill
    {
        class XianSiVerifier : CardsAndTargetsVerifier
        {
            public XianSiVerifier()
            {
                Discarding = true;
                MinCards = 1;
                MaxCards = 1;
                MinPlayers = 1;
                MaxPlayers = 2;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player.HandCards().Count + player.Equipments().Count + player.DelayedTools().Count > 0;
            }
        }

        void GetTheirCards(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("XianSi"), new XianSiVerifier(), out skill, out cards, out players))
            {
                Game.CurrentGame.HandleCardDiscard(Owner, cards);
                Game.CurrentGame.SortByOrderOfComputation(Game.CurrentGame.CurrentPlayer, players);
                NotifySkillUse(players);
                StagingDeckType XianSiTempDeck = new StagingDeckType("XianSi");
                CardsMovement move = new CardsMovement();
                move.Helper.IsFakedMove = true;
                foreach (Player p in players)
                {
                    if (p.HandCards().Count + p.Equipments().Count + p.DelayedTools().Count == 0) continue;
                    List<List<Card>> answer;
                    var deckplaces = new List<DeckPlace>() { new DeckPlace(p, DeckType.Hand), new DeckPlace(p, DeckType.Equipment), new DeckPlace(p, DeckType.DelayedTools) };
                    if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("XianSi"), deckplaces,
                        new List<string>() { "XianSi" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(true), out answer))
                    {
                        answer = new List<List<Card>>();
                        answer.Add(Game.CurrentGame.PickDefaultCardsFrom(deckplaces));
                    }
                    move.Cards = answer[0];
                    move.To = new DeckPlace(p, XianSiTempDeck);
                    Game.CurrentGame.MoveCards(move, false, Core.Utils.GameDelays.None);
                    Game.CurrentGame.PlayerLostCard(p, answer[0]);
                }
                move.Cards.Clear();
                move.Helper.IsFakedMove = false;
                move.To = new DeckPlace(Owner, XianSiDeck);
                foreach (Player p in players)
                {
                    move.Cards.AddRange(Game.CurrentGame.Decks[p, XianSiTempDeck]);
                }
                Game.CurrentGame.SyncImmutableCardsAll(move.Cards);
                cards = new List<Card>(move.Cards);
                Game.CurrentGame.MoveCards(move);
                Game.CurrentGame.NotificationProxy.NotifyActionComplete();
                throw new TriggerResultException(TriggerResult.End);
            }
        }

        public static PrivateDeckType XianSiDeck = new PrivateDeckType("XianSi", false);
        
        public XianSi()
        {
            LinkedSkill = new XianSiDistributor();
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                GetTheirCards,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAutoInvoked = null;
        }

        public class XianSiGivenSkill : CardTransformSkill, IRulerGivenSkill
        {
            public override VerifierResult TryTransform(List<Card> cards, List<Player> targets, out CompositeCard card, bool isPlay)
            {
                card = new CompositeCard();
                card.Type = new Sha();
                if (isPlay) return VerifierResult.Fail;
                if (Game.CurrentGame.Decks[Master, XianSiDeck].Count <= 1) return VerifierResult.Fail;
                if (targets == null || targets.Count == 0) return VerifierResult.Success;
                if (targets.Contains(Master)) return VerifierResult.Success;
                return VerifierResult.Fail;
            }

            protected override bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets, bool isPlay)
            {
                if (Game.CurrentGame.Decks[Master, XianSiDeck].Count > 0)
                {
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>() { Game.CurrentGame.Decks[Master, XianSiDeck][0], Game.CurrentGame.Decks[Master, XianSiDeck][1] };
                    move.To = new DeckPlace(null, DeckType.Discard);
                    Game.CurrentGame.MoveCards(move);
                }
                return true;
            }

            public override List<CardHandler> PossibleResults
            {
                get { return new List<CardHandler>() { new Sha() }; }
            }
            public Player Master { get; set; }
        }

        public class XianSiDistributor : RulerGivenSkillContainerSkill
        {
            public XianSiDistributor()
                : base(new XianSiGivenSkill(), null, true)
            {
            }
        }
    }
}
