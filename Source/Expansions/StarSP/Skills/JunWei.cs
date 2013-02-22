using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using Sanguosha.Expansions.Basic.Cards;
using System.Threading;
using Sanguosha.Core.Utils;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 军威–回合结束阶段开始时，若“锦”的数量达到3或更多，你可以弃置三张“锦”，并选择一名角色，该角色须选择一项：1、展示一张【闪】，然后交给一名由你指定的其他角色；2、失去1点体力，然后令你将其装备区内的一张牌移出游戏，该角色的回合结束后，将移除游戏的牌置入其装备区。
    /// </summary>
    public class JunWei : TriggerSkill
    {
        class JunWeiGiveShanVerifier : CardsAndTargetsVerifier
        {
            public JunWeiGiveShanVerifier()
            {
                MaxCards = 0;
                MinCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
            }
        }

        class JunWeiVerifier : CardUsageVerifier
        {
            public JunWeiVerifier()
            {
                Helper.OtherDecksUsed.Add(YinLing.JinDeck);
            }
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (players != null && players.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count > 3)
                {
                    return VerifierResult.Fail;
                }
                if (cards.Any(c => c.Place.DeckType != YinLing.JinDeck))
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || players == null || players.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards.Count < 3)
                    return VerifierResult.Partial;
                return VerifierResult.Success;
            }
            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }
        }

        class JunWeiShowCardVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
                if (!(cards[0].Type is Shan))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Owner.AskForCardUsage(new CardUsagePrompt("JunWei"), new JunWeiVerifier(), out skill, out cards, out players))
            {
                NotifySkillUse(players);
                Game.CurrentGame.HandleCardDiscard(Owner, cards);
                Player target = players[0];
                if (target.AskForCardUsage(new CardUsagePrompt("JunWeiShowCard"), new JunWeiShowCardVerifier(), out skill, out cards, out players))
                {
                    Card temp = cards[0];
                    Game.CurrentGame.NotificationProxy.NotifyShowCard(target, temp);
                    if (!Owner.AskForCardUsage(new CardUsagePrompt("JunWeiGiveShan"), new JunWeiGiveShanVerifier(), out skill, out cards, out players))
                    {
                        players = new List<Player>() { Owner };
                    }
                    Game.CurrentGame.SyncCardAll(ref temp);
                    Game.CurrentGame.HandleCardTransferToHand(target, players[0], new List<Card>() { temp });
                }
                else
                {
                    Game.CurrentGame.LoseHealth(target, 1);
                    if (target.Equipments().Count == 0) return;
                    List<List<Card>> answer;
                    List<DeckPlace> sourceDecks = new List<DeckPlace>();
                    sourceDecks.Add(new DeckPlace(target, DeckType.Equipment));
                    if (!Owner.AskForCardChoice(new CardChoicePrompt("JunWeiChoice", target, Owner),
                        sourceDecks,
                        new List<string>() { "JunWei" },
                        new List<int>() { 1 },
                        new RequireOneCardChoiceVerifier(),
                        out answer))
                    {
                        answer = new List<List<Card>>();
                        answer.Add(new List<Card>());
                        answer[0].Add(target.Equipments().First());
                    }
                    junweiTarget.Add(target);
                    Game.CurrentGame.HandleCardTransfer(target, target, JunWeiDeck, answer[0]);
                }
            }
        }

        void LoseJunWei(Player Owner)
        {
            DiscardedTempCard();
        }

        void DiscardedTempCard()
        {
            foreach (Player pl in junweiTarget)
            {
                List<Card> tmp = new List<Card>(Game.CurrentGame.Decks[pl, JunWeiDeck]);
                Game.CurrentGame.HandleCardDiscard(pl, tmp);
            }
            junweiTarget.Clear();
        }

        public override Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                Player original = base.Owner;
                base.Owner = value;
                if (base.Owner == null && original != null)
                {
                    LoseJunWei(original);
                }
            }
        }

        public JunWei()
        {
            junweiTarget = new List<Player>();
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[p, YinLing.JinDeck].Count >= 3; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[a.Source, JunWeiDeck].Count > 0; },
                (p, e, a) =>
                {
                    junweiTarget.Remove(a.Source);
                    List<Card> cards = new List<Card>(Game.CurrentGame.Decks[a.Source, JunWeiDeck]);
                    cards.Reverse();
                    foreach (Card c in cards)
                    {
                        (c.Type as Equipment).Install(a.Source, c, null);
                    }
                },
                TriggerCondition.Global
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhasePostEnd, trigger2);

            var trigger3 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { DiscardedTempCard(); },
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerIsDead, trigger3);

            IsAutoInvoked = null;
        }

        List<Player> junweiTarget;
        public static PrivateDeckType JunWeiDeck = new PrivateDeckType("JunWei", true);
    }
}
