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
    /// 巧说-出牌阶段，你可以与一名其他角色拼点，若你赢，你获得以下技能直到回合结束：你使用的下一张非延时类锦囊可以额外指定一个目标或减少指定一个目标。若你没赢，你不能使用非延时类锦囊直到回合结束。每阶段限一次。
    /// </summary>
    public class QiaoShui : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[QiaoShuiUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (Owner.HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets[0] == Owner || Game.CurrentGame.Decks[arg.Targets[0], DeckType.Hand].Count == 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public class QiaoShuiLoseTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                if (eventArgs.Card.Type.IsCardCategory(CardCategory.ImmediateTool))
                {
                    throw new TriggerResultException(TriggerResult.Fail);
                }
            }

            public QiaoShuiLoseTrigger(Player p)
            {
                Owner = p;
            }
        }

        class QiaoShuiRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanUseCard, loseTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }

            Trigger loseTrigger;
            public QiaoShuiRemoval(Player p, Trigger lose)
            {
                Owner = p;
                loseTrigger = lose;
            }
        }

        class QiaoShuiVerifier : CardsAndTargetsVerifier
        {
            CardHandler handler;
            List<Player> existingTargets;
            ICard existingCard;
            public QiaoShuiVerifier(List<Player> p, ICard c, CardHandler handler)
            {
                existingCard = c;
                existingTargets = p;
                this.handler = handler;
                MaxCards = 0;
                MinCards = 0;
                MaxPlayers = Int16.MaxValue;
                MinPlayers = 1;
            }
            protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
            {
                if (cards != null && cards.Count > 0) return false;
                if (players != null && players.Count > 0 && existingTargets.Contains(players[0]))
                {
                    if (players.Count > 1) return false;
                    return true;
                }
                if (players == null || players.Count == 0) return null;
                var actualTargets = handler.ActualTargets(source, players, existingCard);
                if (actualTargets.Count > 1) return false;
                if (existingTargets.Contains(actualTargets[0])) return false;
                var ret = handler.VerifyTargets(source, existingCard, players);
                if (ret == VerifierResult.Partial) return null;
                if (ret == VerifierResult.Fail) return false;
                return true;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return true;
            }
        }
        public class QiaoShuiWinTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner || used)
                {
                    return;
                }
                if (eventArgs.Card.Type.IsCardCategory(CardCategory.ImmediateTool) ||
                    eventArgs.Card.Type.IsCardCategory(CardCategory.Basic))
                {
                    used = true;
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    if (Owner.AskForCardUsage(new CardUsagePrompt("QiaoShui", this), new QiaoShuiVerifier(eventArgs.Targets, eventArgs.Card, eventArgs.Card.Type), out skill, out cards, out players))
                    {
                        theSkill.NotifyAction(Owner, players, new List<Card>());
                        if (eventArgs.Targets.Contains(players[0]))
                        {
                            eventArgs.Targets.Remove(players[0]);
                            
                        }
                        else
                        {
                            eventArgs.UiTargets.AddRange(players);
                            eventArgs.Targets = eventArgs.Card.Type.ActualTargets(eventArgs.Source, eventArgs.UiTargets, eventArgs.Card);
                        }
                    }

                }
            }

            bool used;
            ActiveSkill theSkill;
            public QiaoShuiWinTrigger(Player p, ActiveSkill sk)
            {
                used = false;
                Owner = p;
                theSkill = sk;
            }
        }

        class QiaoShuiWinRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerUsedCard, winTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }

            Trigger winTrigger;
            public QiaoShuiWinRemoval(Player p, Trigger win)
            {
                Owner = p;
                winTrigger = win;
            }
        }
        public override bool Commit(GameEventArgs arg)
        {
            Owner[QiaoShuiUsed] = 1;
            var result = Game.CurrentGame.PinDian(Owner, arg.Targets[0], this);
            if (result == true)
            {
                var winTrigger = new QiaoShuiWinTrigger(Owner, this);
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerUsedCard, winTrigger);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new QiaoShuiWinRemoval(Owner, winTrigger));
            }
            else
            {
                var loseTrigger = new QiaoShuiLoseTrigger(Owner);
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanUseCard, loseTrigger);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new QiaoShuiRemoval(Owner, loseTrigger));
            }
            return true;
        }

        private static PlayerAttribute QiaoShuiUsed = PlayerAttribute.Register("QiaoShuiUsed", true);
    }
}
