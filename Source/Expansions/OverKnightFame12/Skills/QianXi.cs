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

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 潜袭-回合开始阶段开始时，你可以进行一次判定，然后令一名距离为1的角色不能使用或打出与判定结果颜色相同的手牌，直到回合结束。
    /// </summary>
    public class QianXi : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return QianXiEffect;
        }

        class QianXiCannotUsedAndPlayCard : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (Owner != eventArgs.Source)
                {
                    return;
                }
                if (eventArgs.Card.SuitColor == color && eventArgs.Card.Place.DeckType == DeckType.Hand)
                {
                    throw new TriggerResultException(TriggerResult.Fail);
                }
            }

            SuitColorType color;
            public QianXiCannotUsedAndPlayCard(Player player, SuitColorType colorType)
            {
                color = colorType;
                Owner = player;
            }
        }

        class QianXiEffectRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                target[QianXiRed] = 0;
                target[QianXiBlack] = 0;
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanPlayCard, trigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanUseCard, trigger);
            }

            Trigger trigger;
            Player target;
            public QianXiEffectRemoval(Player player, Player target, Trigger trigger)
            {
                Owner = player;
                this.target = target;
                this.trigger = trigger;
            }
        }

        class QianXiVerifier : CardsAndTargetsVerifier
        {
            public QianXiVerifier()
            {
                MaxCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return Game.CurrentGame.DistanceTo(source, player) == 1;
            }
        }

        int QianXiEffect { get; set; }
        public QianXi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    var card = Game.CurrentGame.Judge(Owner, this);
                    if (!Game.CurrentGame.AlivePlayers.Any(player => Game.CurrentGame.DistanceTo(p, player) == 1))
                    {
                        return;
                    }
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    if (Owner.AskForCardUsage(new CardUsagePrompt("QianXi"), new QianXiVerifier(), out skill, out cards, out players))
                    {
                        if (card.SuitColor == SuitColorType.Red)
                        {
                            QianXiEffect = 0;
                            players[0][QianXiRed] = 1;
                        }
                        else if (card.SuitColor == SuitColorType.Black)
                        {
                            QianXiEffect = 1;
                            players[0][QianXiBlack] = 1;
                        }
                        NotifySkillUse(players);
                        Trigger tri = new QianXiCannotUsedAndPlayCard(players[0], card.SuitColor);
                        Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanUseCard, tri);
                        Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanPlayCard, tri);
                        Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new QianXiEffectRemoval(Owner, players[0], tri));
                    }
                },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAutoInvoked = false;
            IsRulerOnly = true;
        }

        private static PlayerAttribute QianXiRed = PlayerAttribute.Register("QianXiRed", false, false, true);
        private static PlayerAttribute QianXiBlack = PlayerAttribute.Register("QianXiBlack", false, false, true);
    }
}
