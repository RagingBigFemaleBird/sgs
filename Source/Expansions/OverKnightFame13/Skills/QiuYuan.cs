using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    /// <summary>
    /// 求援-当你成为杀的目标时，你可以令一名其他角色交给你一张手牌。若此牌不为闪，该角色也成为此杀的目标。
    /// </summary>
    public class QiuYuan : TriggerSkill
    {
        class QiuYuanVerifier : CardsAndTargetsVerifier
        {
            public QiuYuanVerifier()
            {
                MaxCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player.HandCards().Count > 0;
            }
        }

        class QiuYuanGiveCardVerifier : CardsAndTargetsVerifier
        {
            public QiuYuanGiveCardVerifier()
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
        }

        public QiuYuan()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard.Type is Sha; },
                (p, e, a, cards, players) =>
                {
                    ISkill skill;
                    List<Card> nCards;
                    List<Player> nPlayers;
                    Card theCard = null;
                    if (!players[0].AskForCardUsage(new CardUsagePrompt("QiuYuanGiveCard", p), new QiuYuanGiveCardVerifier(), out skill, out nCards, out nPlayers))
                    {
                        theCard = players[0].HandCards().First();
                        Game.CurrentGame.SyncImmutableCardAll(theCard);
                        nCards.Add(theCard);
                    }
                    else
                    {
                        theCard = nCards.First();
                    }
                    Game.CurrentGame.HandleCardTransferToHand(players[0], p, nCards);
                    if (!(theCard.Type is Shan) && !a.Targets.Contains(players[0]))
                    {
                        a.Targets.Add(players[0]);
                        if (Game.CurrentGame.OrderOf(Game.CurrentGame.CurrentPlayer, p) < Game.CurrentGame.OrderOf(Game.CurrentGame.CurrentPlayer, players[0]))
                        {
                            GameEventArgs newArgs = new GameEventArgs();
                            newArgs.Source = a.Source;
                            newArgs.UiTargets = players;
                            newArgs.Targets = players;
                            newArgs.Card = a.Card;
                            newArgs.ReadonlyCard = a.ReadonlyCard;
                            newArgs.InResponseTo = a.InResponseTo;
                            Game.CurrentGame.Emit(GameEvent.CardUsageTargetConfirming, newArgs);
                        }
                    }
                },
                TriggerCondition.OwnerIsTarget,
                new QiuYuanVerifier()
            ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.CardUsageTargetConfirming, trigger);
            IsAutoInvoked = null;
        }
    }
}
