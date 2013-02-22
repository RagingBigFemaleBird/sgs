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
    /// 笔伐-回合结束阶段开始时，你可以将一张手牌移出游戏并指定一名其他角色。该角色的回合开始时，其观看你移出游戏的牌并选择一项：交给你一张与此牌同类型的手牌并获得此牌；或将此牌置入弃牌堆，然后失去1点体力。
    /// </summary>
    public class BiFa : TriggerSkill
    {
        class BiFaVerifier : CardsAndTargetsVerifier
        {
            public BiFaVerifier()
            {
                MaxPlayers = 1;
                MinPlayers = 1;
                MaxCards = 1;
                MinCards = 1;
                Helper.NoCardReveal = true;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != source && Game.CurrentGame.Decks[player, BiFaDeck].Count == 0;
            }
        }

        class BiFaGiveCardVerifier : CardsAndTargetsVerifier
        {
            Card theCard;
            public BiFaGiveCardVerifier(Card card)
            {
                MaxPlayers = 0;
                MaxCards = 1;
                MinCards = 1;
                this.theCard = card;
                Helper.OtherDecksUsed.Add(BiFaDeck);
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                if (card == theCard || card.Place.DeckType != DeckType.Hand) return false;
                return card.Type.BaseCategory() == theCard.Type.BaseCategory();
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
        {
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(players[0], BiFaDeck);
            move.Helper = new MovementHelper();
            move.Helper.IsFakedMove = true;
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerLostCard(Owner, cards);
            Game.CurrentGame.RegisterTrigger(GameEvent.PhaseBeginEvents[TurnPhase.Start], new BiFaTrigger(players[0], this));
        }

        class BiFaTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner) return;
                if (Game.CurrentGame.Decks[Owner, BiFaDeck].Count == 1)
                {
                    Card theCard = Game.CurrentGame.Decks[Owner, BiFaDeck][0];
                    Player ChenLin = theCard.HistoryPlace1.Player;
                    Game.CurrentGame.SyncImmutableCardAll(theCard);
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>() { theCard };
                    move.To = new DeckPlace(Owner, BiFaDeck);
                    move.Helper = new MovementHelper();
                    move.Helper.IsFakedMove = true;
                    Game.CurrentGame.MoveCards(move);

                    ActionLog log = new ActionLog();
                    log.GameAction = GameAction.None;
                    log.SkillAction = bifa;
                    log.Source = ChenLin;
                    log.SkillSoundOnly = true;

                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    if (!ChenLin.IsDead && Owner.AskForCardUsage(new CardUsagePrompt("BiFaGiveCard", ChenLin), new BiFaGiveCardVerifier(theCard), out skill, out cards, out players))
                    {
                        log.SpecialEffectHint = 2;
                        Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
                        Game.CurrentGame.NotificationProxy.NotifyLogEvent(new LogEvent("BiFa", Owner, bifa, LogEventArg.Fail), new List<Player>() { Owner });
                        Game.CurrentGame.HandleCardTransferToHand(Owner, ChenLin, cards);
                        Game.CurrentGame.HandleCardTransferToHand(Owner, Owner, new List<Card>() { theCard });
                    }
                    else
                    {
                        log.SpecialEffectHint = 1;
                        Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
                        Game.CurrentGame.NotificationProxy.NotifyLogEvent(new LogEvent("BiFa", Owner, bifa, LogEventArg.Success), new List<Player>() { Owner });
                        theCard.Log = new ActionLog();
                        theCard.Log.SkillAction = bifa;
                        theCard.Log.GameAction = GameAction.PlaceIntoDiscard;
                        Game.CurrentGame.PlaceIntoDiscard(null, new List<Card>() { theCard });
                        Game.CurrentGame.LoseHealth(Owner, 1);
                    }
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseBeginEvents[TurnPhase.Start], this);
            }
            ISkill bifa;
            public BiFaTrigger(Player p, ISkill skill)
            {
                Owner = p;
                bifa = skill;
                Priority = int.MaxValue;
            }
        }

        public BiFa()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource,
                new BiFaVerifier()
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
            IsAutoInvoked = null;
        }

        public static PrivateDeckType BiFaDeck = new PrivateDeckType("BiFa", false);
    }
}
