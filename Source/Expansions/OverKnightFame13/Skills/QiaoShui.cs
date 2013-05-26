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
    /// 巧说-出牌阶段开始时，你可与一名其他角色拼点。若你赢，你使用的下一张基本牌或非延时类锦囊牌可以额外指定任意一名其他角色为目标或减少指定一个目标；若你没赢，你不能使用锦囊牌直到回合结束。每阶段限一次。
    /// </summary>
    public class QiaoShui : TriggerSkill
    {
        public QiaoShui()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                    this,
                    (p, e, a) => { return p.HandCards().Count > 0; },
                    (p, e, a, c, t) =>
                    {
                        var result = Game.CurrentGame.PinDian(Owner, t[0], this);
                        if (result == true)
                        {
                            var winTrigger = new QiaoShuiWinTrigger(p, this);
                            Game.CurrentGame.RegisterTrigger(GameEvent.PlayerUsedCard, winTrigger);
                            Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new QiaoShuiWinRemoval(Owner, winTrigger));
                        }
                        else
                        {
                            var loseTrigger = new QiaoShuiLoseTrigger(Owner);
                            Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanUseCard, loseTrigger);
                            Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new QiaoShuiRemoval(Owner, loseTrigger));
                        }
                    },
                    TriggerCondition.OwnerIsSource,
                    new PinDianVerifier()
                );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Play], trigger);
            IsAutoInvoked = null;
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
                MaxPlayers = 1;
                MinPlayers = 1;
            }
            protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
            {
                if (cards != null && cards.Count > 0) return false;
                if (players != null && players.Count > 0 && existingTargets.Contains(players[0]))
                {
                    return true;
                }
                if (players == null || players.Count == 0) return null;
                if (existingTargets.Contains(players[0])) return false;
                var ret = handler.Verify(source, existingCard, players, true);
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
                    if (Owner.AskForCardUsage(new CardUsagePrompt("QiaoShuiWin", this), new QiaoShuiVerifier(eventArgs.Targets, eventArgs.Card, eventArgs.Card.Type), out skill, out cards, out players))
                    {
                        theSkill.NotifySkillUse(players);
                        if (eventArgs.Targets.Contains(players[0]))
                        {
                            eventArgs.Targets.Remove(players[0]);                            
                        }
                        else
                        {
                            eventArgs.Targets.AddRange(players);
                        }
                    }

                }
            }

            bool used;
            TriggerSkill theSkill;
            public QiaoShuiWinTrigger(Player p, TriggerSkill sk)
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
    }
}
